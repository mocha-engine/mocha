using Mocha.Common;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

/// <summary>
/// A wrapper for an external assembly to be loaded.
/// </summary>
/// <typeparam name="TEntryPoint">The type to retrieve from the assembly as its entry point.</typeparam>
internal sealed class ProjectAssembly<TEntryPoint> where TEntryPoint : IGame
{
	/// <summary>
	/// The loaded assembly.
	/// </summary>
	internal Assembly Assembly { get; private set; } = null!;
	/// <summary>
	/// The found entry point into the assembly.
	/// </summary>
	internal TEntryPoint EntryPoint { get; private set; } = default!;

	private readonly ProjectAssemblyInfo _projectAssemblyInfo;
	private FileSystemWatcher _projWatcher = null!;
	private FileSystemWatcher _codeWatcher = null!;
	private AssemblyLoadContext _loadContext;

	private TimeSince _timeSinceCsProjChange;
	private TimeSince _timeSinceLastFileChange;
	private Task _buildTask;
	private bool _buildRequested;

	internal ProjectAssembly( in ProjectAssemblyInfo assemblyInfo )
	{
		_projectAssemblyInfo = assemblyInfo;
		_loadContext = new AssemblyLoadContext( null, isCollectible: true );

		/*
		 * Mocha.Common must always be "Private" (ie. "Copy Local" as "No" in the reference's properties)
		 * otherwise **the engine will load Mocha.Common twice incorrectly, causing a mismatch between types**.
		 * If you ever need to delete and re-add this reference please make sure this is always the case.
		 * 
		 * If you didn't do that then you'll get an exception around here!
		 */
		_buildTask = Build();
		_buildTask.Wait();
		CreateFileWatchers();
	}

	/// <summary>
	/// Unloads the current assembly and swaps it with a new one.
	/// </summary>
	/// <param name="newAssembly">The new assembly to swap in.</param>
	/// <param name="newEntryPoint">The new entry point to the assembly.</param>
	private void Swap( Assembly newAssembly, TEntryPoint newEntryPoint )
	{
		_loadContext.Unload();
		_loadContext = new AssemblyLoadContext( null, isCollectible: true );

		Assembly = newAssembly;
		EntryPoint = newEntryPoint;
	}

	/// <summary>
	/// Builds the projects assembly.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	private async Task Build()
	{
		var realmString = _projectAssemblyInfo.IsServer ? "Server" : "Client";
		var assemblyName = $"'{_projectAssemblyInfo.AssemblyName}' ({realmString})";

		Notify.AddNotification( $"Building...", $"Compiling {assemblyName}", FontAwesome.Spinner );
		var compileResult = await Compiler.Compile( _projectAssemblyInfo );

		if ( !compileResult.WasSuccessful )
		{
			var errorStr = string.Join( '\n', compileResult.Errors! );

			foreach ( var error in compileResult.Errors! )
				Log.Error( error );

			Notify.AddError( $"Build failed", $"Failed to compile {assemblyName}\n{errorStr}", FontAwesome.FaceSadTear );
			return;
		}

		// Keep old assembly as reference. Should be destroyed once out of scope
		var oldAssembly = Assembly;
		var oldGameInterface = EntryPoint;

		// Load new assembly
		var assemblyStream = new MemoryStream( compileResult.CompiledAssembly! );
		var symbolsStream = compileResult.HasSymbols ? new MemoryStream( compileResult.CompiledAssemblySymbols! ) : null;

		var newAssembly = _loadContext.LoadFromStream( assemblyStream, symbolsStream );
		var newInterface = CreateEntryPointFromAssembly( newAssembly );

		// Invoke upgrader to move values from oldAssembly into assembly
		if ( oldAssembly != null && oldGameInterface != null )
		{
			Upgrader.UpgradedReferences.Clear();

			UpgradeEntities( oldAssembly, newAssembly );

			Upgrader.UpgradeInstance( oldGameInterface, newInterface );

			ConsoleSystem.Internal.ClearGameCVars();
		}

		// Now that everything's been upgraded, swap the new interface
		// and assembly in
		Swap( newAssembly, newInterface );

		Notify.AddNotification( $"Build successful!", $"Compiled {assemblyName}!", FontAwesome.FaceGrinStars );

		ConsoleSystem.Internal.RegisterAssembly( newAssembly, extraFlags: CVarFlags.Game );

		Event.Run( Event.Game.HotloadAttribute.Name );

		if ( !_buildRequested )
			return;

		_buildRequested = false;
		await Build();
	}

	/// <summary>
	/// Upgrades all entities that were affected by the swap.
	/// </summary>
	/// <param name="oldAssembly">The old assembly being unloaded.</param>
	/// <param name="newAssembly">The new assembly being loaded.</param>
	private static void UpgradeEntities( Assembly oldAssembly, Assembly newAssembly )
	{
		var entityRegistryCopy = EntityRegistry.Instance.ToList();

		for ( int i = 0; i < entityRegistryCopy.Count; i++ )
		{
			var entity = entityRegistryCopy[i];
			var entityType = entity.GetType();

			// Do we actually want to upgrade this? If not, skip.
			if ( entityType.Assembly != oldAssembly )
				continue;

			// Unregister the old entity
			EntityRegistry.Instance.UnregisterEntity( entity );

			// Find new type for entity in new assembly
			var newType = newAssembly.GetType( entityType.FullName ?? entityType.Name )!;
			var newEntity = (IEntity)FormatterServices.GetUninitializedObject( newType )!;

			// Have we already upgraded this?
			if ( Upgrader.UpgradedReferences.TryGetValue( entity.GetHashCode(), out var upgradedValue ) )
			{
				newEntity = (IEntity)upgradedValue;
			}
			else
			{
				Upgrader.UpgradedReferences[entity.GetHashCode()] = newEntity;
				Upgrader.UpgradeInstance( entity, newEntity );
			}

			// If we created a new entity successfully, register it
			if ( newEntity is not null )
				EntityRegistry.Instance.RegisterEntity( newEntity );
		}
	}

	/// <summary>
	/// Finds and creates an entry point from the project assembly.
	/// </summary>
	/// <param name="assembly">The assembly to search.</param>
	/// <returns>The created entry point from the project assembly.</returns>
	/// <exception cref="EntryPointNotFoundException">Thrown when no valid entry point was found.</exception>
	private static TEntryPoint CreateEntryPointFromAssembly( Assembly assembly )
	{
		var tType = typeof( TEntryPoint );
		// Find first type that derives from interface T
		foreach ( var type in assembly.GetTypes() )
		{
			if ( type.GetInterface( tType.FullName ?? tType.Name ) is not null )
				return (TEntryPoint)Activator.CreateInstance( type )!;
		}

		throw new EntryPointNotFoundException( $"Could not find implementation of '{tType.Name}'" );
	}

	/// <summary>
	/// Creates the file system watcher to check for file changes in the project.
	/// </summary>
	/// <param name="sourcePath"></param>
	private void CreateFileWatchers()
	{
		_projWatcher = new FileSystemWatcher(
			Path.GetDirectoryName( _projectAssemblyInfo.ProjectPath )!,
			Path.GetFileName( _projectAssemblyInfo.ProjectPath ) )
		{
			NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size
		};

		_projWatcher.Changed += OnCsProjChanged;
		_projWatcher.EnableRaisingEvents = true;

		_codeWatcher = new FileSystemWatcher( _projectAssemblyInfo.SourceRoot, "*.cs" )
		{
			NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size
		};

		// Visual Studio will create a new temporary file, write to it,
		// delete the cs file and then rename the temporary file to
		// match the deleted file
		// The Renamed event will catch this nicely for us, because renaming
		// is the last thing that happens in the order of operations

		// This will typically happen twice, so we'll gate it with a TimeSince too
		_codeWatcher.Renamed += OnFileChanged;
		_codeWatcher.IncludeSubdirectories = true;
		_codeWatcher.EnableRaisingEvents = true;
	}

	/// <summary>
	/// Invoked when the csproj has changed.
	/// </summary>
	private void OnCsProjChanged( object sender, FileSystemEventArgs e )
	{
		// This will typically fire twice, so gate it with a TimeSince
		if ( _timeSinceCsProjChange < 1 )
			return;

		_timeSinceCsProjChange = 0;

		if ( _buildTask.IsCompleted )
			_buildTask = Build();
		else
			_buildRequested = true;
	}

	/// <summary>
	/// Invoked when a file system change has occurred.
	/// </summary>
	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		// This will typically fire twice, so gate it with a TimeSince
		if ( _timeSinceLastFileChange < 1 )
			return;

		// This might be a directory - if it is then skip
		if ( string.IsNullOrEmpty( Path.GetExtension( e.FullPath ) ) )
			return;

		_timeSinceLastFileChange = 0f;

		if ( _buildTask.IsCompleted )
			_buildTask = Build();
		else
			_buildRequested = true;
	}
}
