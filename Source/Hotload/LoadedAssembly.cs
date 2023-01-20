using System.Reflection;

namespace Mocha;

public class LoadedAssemblyType<T>
{
	private T? managedClass;

	private FileSystemWatcher watcher;

	public LoadedAssemblyType( string dllPath, string sourcePath )
	{
		LoadManagedClass( dllPath );
		CreateFileSystemWatcher( sourcePath );
	}

	private void LoadManagedClass( string dllPath )
	{
		var name = AssemblyName.GetAssemblyName( dllPath );
		var currentAssembly = AppDomain.CurrentDomain.Load( name );

		// Find first type that derives from T
		foreach ( var type in currentAssembly.GetTypes() )
		{
			if ( type.GetInterface( typeof( T ).FullName! ) != null )
			{
				managedClass = (T)Activator.CreateInstance( type )!;
				break;
			}
		}

		if ( managedClass == null )
		{
			throw new Exception( "Could not find IGame implementation" );
		}
	}

	private void CreateFileSystemWatcher( string sourcePath )
	{
		watcher = new FileSystemWatcher( sourcePath, "*.*" );
		watcher.Changed += OnFileChanged;
		watcher.EnableRaisingEvents = true;
	}

	private void OnFileChanged( object sender, FileSystemEventArgs e )
	{
		Log.Trace( $"File {e.FullPath} was changed" );
	}

	public T Value => managedClass!;

	public static implicit operator T( LoadedAssemblyType<T> loadedAssemblyType )
	{
		return loadedAssemblyType.managedClass!;
	}
}
