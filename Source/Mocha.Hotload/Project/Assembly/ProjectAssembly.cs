using Microsoft.CodeAnalysis;
using Mocha.Common;
using Mocha.Hotload.Compilation;
using Mocha.Hotload.Upgrading;
using Mocha.Hotload.Util;
using System.Reflection;
using System.Runtime.Loader;

namespace Mocha.Hotload.Projects;

/// <summary>
/// A wrapper for an external assembly to be loaded.
/// </summary>
/// <typeparam name="TEntryPoint">The type to retrieve from the assembly as its entry point.</typeparam>
internal sealed class ProjectAssembly<TEntryPoint> where TEntryPoint : IGame
{
	/// <summary>
	/// The loaded assembly.
	/// </summary>
	internal Assembly? Assembly { get; private set; } = null!;
	/// <summary>
	/// The found entry point into the <see cref="Assembly"/>.
	/// </summary>
	internal TEntryPoint? EntryPoint { get; private set; } = default!;

	/// <summary>
	/// The information for this specific <see cref="Assembly"/>.
	/// </summary>
	private readonly ProjectAssemblyInfo _projectAssemblyInfo;
	/// <summary>
	/// The <see cref="Workspace"/> that is used for incremental compilation of the project.
	/// </summary>
	private AdhocWorkspace _workspace = null!;
	/// <summary>
	/// The <see cref="FileSystemWatcher"/> that is responsible for checking when the csproj file changes.
	/// </summary>
	private FileSystemWatcher _projWatcher = null!;
	/// <summary>
	/// The <see cref="FileSystemWatcher"/> that is responsible for checking when a code file changes.
	/// </summary>
	private FileSystemWatcher _codeWatcher = null!;
	/// <summary>
	/// The load context for the <see cref="Assembly"/>.
	/// </summary>
	private AssemblyLoadContext _loadContext;

	/// <summary>
	/// The time since the last change happened to the csproj.
	/// </summary>
	private TimeSince _timeSinceCsProjChange;
	/// <summary>
	/// The <see cref="Task"/> that represents the current build process.
	/// </summary>
	private Task _buildTask;
	/// <summary>
	/// Whether or not a full build has been requested.
	/// </summary>
	private bool _buildRequested;
	/// <summary>
	/// Whether or not an incremental build has been requested.
	/// </summary>
	private bool _incrementalBuildRequested;
	/// <summary>
	/// A container for all the changed files and the specific change that occurred to them.
	/// </summary>
	private readonly Dictionary<string, WatcherChangeTypes> _incrementalBuildChanges = new();

	/// <summary>
	/// Initializes a new instance of <see cref="ProjectAssembly{TEntryPoint}"/>.
	/// </summary>
	/// <param name="assemblyInfo">The information needed to create the <see cref="System.Reflection.Assembly"/>.</param>
	internal ProjectAssembly( in ProjectAssemblyInfo assemblyInfo )
	{
		_projectAssemblyInfo = assemblyInfo;
		_loadContext = new AssemblyLoadContext( assemblyInfo.AssemblyName, true );

		/*
		 * Mocha.Common must always be "Private" (ie. "Copy Local" as "No" in the reference's properties)
		 * otherwise **the engine will load Mocha.Common twice incorrectly, causing a mismatch between types**.
		 * If you ever need to delete and re-add this reference please make sure this is always the case.
		 * 
		 * If you didn't do that then you'll get an exception around here!
		 */
		_buildTask = BuildAsync();
		_buildTask.Wait();

		CreateFileWatchers();
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ProjectAssembly{TEntryPoint}"/>.
	/// NOTE: This constructor will not setup any of the hotloading features. Use this for distribution.
	/// </summary>
	/// <param name="assembly">The assembly </param>
	internal ProjectAssembly( Assembly assembly )
	{
		Assembly = assembly;
		EntryPoint = CreateEntryPointFromAssembly( assembly );

		_loadContext = null!;
		_buildTask = null!;
	}

	/// <summary>
	/// Builds the projects <see cref="Assembly"/>.
	/// </summary>
	/// <param name="incremental">Whether or not the compilation should be incremental.</param>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
	private async Task BuildAsync( bool incremental = false )
	{
		// Start stopwatch.
		using var _ = new Stopwatch( incremental
			? $"{_projectAssemblyInfo.AssemblyName} incremental build and hotload"
			: $"{_projectAssemblyInfo.AssemblyName} build and hotload" );

		// Notify the build is starting.
		var realmString = _projectAssemblyInfo.IsServer ? "Server" : "Client";
		var assemblyName = $"'{_projectAssemblyInfo.AssemblyName}' ({realmString})";

		Notify.AddNotification( $"Building...", $"Compiling {assemblyName}", FontAwesome.Spinner );

		// Compile.
		CompileResult compileResult;
		if ( incremental )
		{
			var changedFiles = new Dictionary<string, WatcherChangeTypes>( _incrementalBuildChanges );
			_incrementalBuildChanges.Clear();

			compileResult = await Compiler.IncrementalCompileAsync( _workspace, changedFiles, _projectAssemblyInfo );
		}
		else
			compileResult = await Compiler.CompileAsync( _projectAssemblyInfo );

		// Check if compile failed.
		if ( !compileResult.WasSuccessful )
		{
			var errorStr = string.Join( '\n', compileResult.Errors! );

			foreach ( var error in compileResult.Errors! )
				Log.Error( error );

			Notify.AddError( $"Build failed", $"Failed to compile {assemblyName}\n{errorStr}", FontAwesome.FaceSadTear );
			// Check if another build is queued before bailing.
			await PostBuildAsync();
			return;
		}

		// Update compile workspace.
		_workspace = compileResult.Workspace!;

		// Swap and upgrade the assemblies.
		Swap( compileResult );

		// Notify the build is finished.
		Notify.AddNotification( $"Build successful!", $"Compiled {assemblyName}!", FontAwesome.FaceGrinStars );

		// Check if another build is queued.
		await PostBuildAsync();
	}

	/// <summary>
	/// Swaps the old <see cref="Assembly"/> with the newly compiled version.
	/// </summary>
	/// <param name="compileResult">The <see cref="CompileResult"/> that contains the new assembly bytes.</param>
	private void Swap( in CompileResult compileResult )
	{
		// Keep old assembly as reference. Should be destroyed once out of scope
		var oldAssembly = Assembly;
		var oldEntryPoint = EntryPoint;

		// Load new assembly
		var assemblyStream = new MemoryStream( compileResult.CompiledAssembly! );
		var symbolsStream = compileResult.HasSymbols ? new MemoryStream( compileResult.CompiledAssemblySymbols! ) : null;

		var newAssembly = _loadContext.LoadFromStream( assemblyStream, symbolsStream );
		var newEntryPoint = CreateEntryPointFromAssembly( newAssembly );

		// Invoke upgrader to move values from oldAssembly into assembly
		if ( oldAssembly is not null && oldEntryPoint is not null )
		{
			Upgrader.Upgrade( oldAssembly, newAssembly, oldEntryPoint, newEntryPoint );
			ConsoleSystem.Internal.ClearGameCVars();
		}

		// Now that everything's been upgraded, swap the new interface
		// and assembly in
		_loadContext.Unload();
		_loadContext = new AssemblyLoadContext( null, isCollectible: true );

		Assembly = newAssembly;
		EntryPoint = newEntryPoint;

		// Re-register the assembly to the console system.
		ConsoleSystem.Internal.RegisterAssembly( Assembly, extraFlags: CVarFlags.Game );

		Event.Run( Event.Game.HotloadAttribute.Name );
	}

	/// <summary>
	/// Checks if another build needs to be ran after the previous one just finished.
	/// </summary>
	/// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
	private async Task PostBuildAsync()
	{
		if ( !_buildRequested && !_incrementalBuildRequested )
			return;

		// Need a full build.
		if ( _buildRequested )
		{
			_buildRequested = false;
			_incrementalBuildRequested = false;
			await BuildAsync();
		}
		// Need an incremental build.
		else
		{
			_incrementalBuildRequested = false;
			await BuildAsync( incremental: true );
		}
	}

	/// <summary>
	/// Finds and creates a <see cref="TEntryPoint"/> from the project assembly.
	/// </summary>
	/// <param name="assembly">The <see cref="Assembly"/> to search.</param>
	/// <returns>The created <see cref="TEntryPoint"/> from the project assembly.</returns>
	/// <exception cref="EntryPointNotFoundException">Thrown when no valid <see cref="TEntryPoint"/> was found.</exception>
	private static TEntryPoint CreateEntryPointFromAssembly( Assembly assembly )
	{
		var tType = typeof( TEntryPoint );
		// Find first type that derives from interface T
		foreach ( var type in assembly.GetTypes() )
		{
			if ( type.IsAssignableTo( tType ) )
				return (TEntryPoint)Activator.CreateInstance( type )!;
		}

		throw new EntryPointNotFoundException( $"Could not find implementation of '{tType.Name}'" );
	}

	/// <summary>
	/// Creates the <see cref="FileSystemWatcher"/> to check for file changes in the project.
	/// </summary>
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
	private async void OnCsProjChanged( object sender, FileSystemEventArgs e )
	{
		// This will typically fire twice, so gate it with a TimeSince.
		if ( _timeSinceCsProjChange < 1 )
			return;

		// If a dedicated server and client are running. There's a chance the csproj is already in use.
		while ( FileUtil.IsFileInUse( e.FullPath ) )
			await Task.Delay( 1 );

		// Bail if the csproj actually didn't change.
		var oldProject = CSharpProject.FromFile( e.FullPath );
		CSharpProject.RemoveCachedProject( e.FullPath );
		var newProject = CSharpProject.FromFile( e.FullPath );
		if ( oldProject == newProject )
			return;

		_timeSinceCsProjChange = 0;

		if ( _buildTask.IsCompleted )
			_buildTask = BuildAsync();
		else
			_buildRequested = true;
	}

	/// <summary>
	/// Invoked when a file system change has occurred.
	/// </summary>
	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		// This might be a directory, if it is then skip.
		if ( Directory.Exists( e.FullPath ) )
			return;

		switch ( e.ChangeType )
		{
			// A C# file was created.
			case WatcherChangeTypes.Created:
				{
					// If a change already exists and is not the file being created then switch to changed.
					if ( _incrementalBuildChanges.TryGetValue( e.FullPath, out var val ) && val != WatcherChangeTypes.Created )
							_incrementalBuildChanges[e.FullPath] = WatcherChangeTypes.Changed;
					// Add created event if it does not exist in the changes.
					else if ( !_incrementalBuildChanges.ContainsKey( e.FullPath ) )
						_incrementalBuildChanges.Add( e.FullPath, WatcherChangeTypes.Created );

					break;
				}
			// A C# file was deleted.
			case WatcherChangeTypes.Deleted:
				{
					if ( _incrementalBuildChanges.TryGetValue( e.FullPath, out var val ) )
					{
						// If the change that currently exists is it being created then just remove the change.
						if ( val == WatcherChangeTypes.Created )
							_incrementalBuildChanges.Remove( e.FullPath );
						// Overwrite any previous change and set it to be deleted.
						else
							_incrementalBuildChanges[e.FullPath] = WatcherChangeTypes.Deleted;
					}
					else if ( !_incrementalBuildChanges.ContainsKey( e.FullPath ) )
						_incrementalBuildChanges.Add( e.FullPath, WatcherChangeTypes.Deleted );

					break;
				}
			// A C# file was changed/renamed.
			case WatcherChangeTypes.Changed:
			case WatcherChangeTypes.Renamed:
				{
					if ( _incrementalBuildChanges.TryGetValue( e.FullPath, out var val ) )
					{
						// If the file was previously created then keep that.
						if ( val == WatcherChangeTypes.Created )
							break;
						// Overwrite any other state with changed.
						else
							_incrementalBuildChanges[e.FullPath] = WatcherChangeTypes.Changed;
					}
					else
						_incrementalBuildChanges.Add( e.FullPath, WatcherChangeTypes.Changed );

					break;
				}
		}

		// Queue build.
		if ( _buildTask.IsCompleted )
			_buildTask = BuildAsync( incremental: true );
		else
			_incrementalBuildRequested = true;
	}
}
