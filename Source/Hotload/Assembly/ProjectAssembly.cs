using Mocha.Common;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization;

namespace Mocha.Hotload;

public class ProjectAssembly<T>
{
	private readonly ProjectAssemblyInfo _projectAssemblyInfo;

	private T? _managedClass;
	private FileSystemWatcher _watcher;
	private Assembly _assembly;
	private AssemblyLoadContext _loadContext;

	public T? Value => _managedClass;
	public Assembly Assembly => _assembly;

	public ProjectAssembly( ProjectAssemblyInfo assemblyInfo )
	{
		_projectAssemblyInfo = assemblyInfo;
		_loadContext = new AssemblyLoadContext( null, isCollectible: true );

		CompileIntoMemory();
		CreateFileSystemWatcher( assemblyInfo.SourceRoot );
	}

	private void Swap( Assembly newAssembly, T newInterface )
	{
		_loadContext?.Unload();
		_loadContext = new AssemblyLoadContext( null, isCollectible: true );

		_assembly = newAssembly;
		_managedClass = newInterface;
	}

	private void CompileIntoMemory()
	{
		Notify.AddNotification( $"Building...", $"Compiling '{_projectAssemblyInfo.AssemblyName}'", FontAwesome.Spinner );
		var compileResult = Compiler.Instance.Compile( _projectAssemblyInfo );

		if ( !compileResult.WasSuccessful )
		{
			var errorStr = string.Join( '\n', compileResult.Errors! );

			foreach ( var error in compileResult.Errors! )
			{
				Log.Error( error );
			}

			Notify.AddError( $"Build failed", $"Failed to compile '{_projectAssemblyInfo.AssemblyName}'\n{errorStr}", FontAwesome.FaceSadTear );
			return;
		}

		// Keep old assembly as reference. Should be destroyed once out of scope
		var oldAssembly = _assembly;
		var oldGameInterface = _managedClass;

		// Load new assembly
		var assemblyStream = new MemoryStream( compileResult.CompiledAssembly! );
		var symbolsStream = compileResult.HasSymbols ? new MemoryStream( compileResult.CompiledAssemblySymbols! ) : null;

		var newAssembly = _loadContext.LoadFromStream( assemblyStream, symbolsStream );
		var newInterface = CreateInterfaceFromAssembly( newAssembly );

		// Invoke upgrader to move values from oldAssembly into assembly
		if ( oldAssembly != null && oldGameInterface != null )
		{
			UpgradeEntities( oldAssembly, newAssembly );

			Upgrader.UpgradeInstance( oldGameInterface, newInterface );

			// Unregister events for old interface
			Event.Unregister( oldGameInterface );
		}

		// Now that everything's been upgraded, swap the new interface
		// and assembly in
		Swap( newAssembly, newInterface );

		Notify.AddNotification( $"Build successful!", $"Compiled '{_projectAssemblyInfo.AssemblyName}'!", FontAwesome.FaceGrinStars );
		Event.Run( Event.Game.HotloadAttribute.Name );
	}

	private void UpgradeEntities( Assembly oldAssembly, Assembly newAssembly )
	{
		Log.Trace( $"Uprading {EntityRegistry.Instance.Count()} entities..." );

		var entityRegistryCopy = EntityRegistry.Instance.ToList();

		for ( int i = 0; i < entityRegistryCopy.Count; i++ )
		{
			var entity = entityRegistryCopy[i];

			Log.Trace( $"Entity {entity.Name}" );

			// Do we actually want to upgrade this?
			if ( entity.GetType().Assembly != oldAssembly )
			{
				Log.Trace( $"\tNot part of hotloaded assembly - skipping" );
				continue;
			}

			// Unregister the old entity
			Log.Trace( $"\tUnregistering old entity {entity.Name}" );
			EntityRegistry.Instance.UnregisterEntity( entity );

			// Find new type for entity in new assembly
			Log.Trace( $"\tCreating new entity instance" );
			var newType = newAssembly.GetType( entity.GetType().FullName )!;
			var newEntity = (IEntity)FormatterServices.GetUninitializedObject( newType )!;

			Log.Trace( $"\tOld type is from assembly hash {entity.GetType().Assembly.GetHashCode()}" );
			Log.Trace( $"\tNew type is from assembly hash {newType.Assembly.GetHashCode()}" );

			Log.Trace( $"\tUpgrading new entity instance" );


			// Have we already upgraded this? If so, use a reference so that we're not 
			// duplicating the object.
			if ( ClassUpgrader.UpgradedReferences.TryGetValue( entity.GetHashCode(), out var upgradedValue ) )
			{
				newEntity = (IEntity)upgradedValue;
			}
			else
			{
				ClassUpgrader.UpgradedReferences[entity.GetHashCode()] = newEntity;
				Upgrader.UpgradeInstance( entity, newEntity );
			}

			// If we created a new entity successfully, register it
			if ( newEntity != null )
			{
				Log.Trace( $"\tRegistering new entity {newEntity.Name}" );
				EntityRegistry.Instance.RegisterEntity( newEntity );
			}
		}
	}

	private T CreateInterfaceFromAssembly( Assembly assembly )
	{
		// Is T an interface?
		if ( typeof( T ).IsInterface )
		{
			// Find first type that derives from interface T
			foreach ( var type in assembly.GetTypes() )
			{
				if ( type.GetInterface( typeof( T ).FullName! ) != null )
				{
					return (T)Activator.CreateInstance( type )!;
				}
			}
		}

		throw new Exception( $"Could not find implementation of '{typeof( T ).Name}'" );
	}

	private void CreateFileSystemWatcher( string sourcePath )
	{
		_watcher = new FileSystemWatcher( sourcePath, "*.cs" );
		_watcher.NotifyFilter = NotifyFilters.Attributes
							 | NotifyFilters.CreationTime
							 | NotifyFilters.DirectoryName
							 | NotifyFilters.FileName
							 | NotifyFilters.LastAccess
							 | NotifyFilters.LastWrite
							 | NotifyFilters.Security
							 | NotifyFilters.Size;

		// Visual Studio will create a new temporary file, write to it,
		// delete the cs file and then rename the temporary file to
		// match the deleted file
		// The Renamed event will catch this nicely for us, because renaming
		// is the last thing that happens in the order of operations

		// This will typically happen twice, so we'll gate it with a TimeSince too
		_watcher.Renamed += OnFileChanged;
		_watcher.IncludeSubdirectories = true;
		_watcher.EnableRaisingEvents = true;
	}

	private TimeSince _timeSinceLastChange;

	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		Log.Trace( $"File {e.FullPath} was changed ({_timeSinceLastChange})" );

		// This will typically fire twice, so gate it with a TimeSince
		if ( _timeSinceLastChange < 1f )
			return;

		// This might be a directory - if it is then skip
		if ( string.IsNullOrEmpty( Path.GetExtension( e.FullPath ) ) )
			return;

		_timeSinceLastChange = 0f;
		CompileIntoMemory();
	}
}
