namespace MochaTool.AssetCompiler;

public class RuntimeAssetCompiler : AssetCompilerBase
{
	private List<FileSystemWatcher> _watchers { get; set; }

	public RuntimeAssetCompiler()
	{
		_watchers = FileSystem.Mounted.CreateMountedFileSystemWatchers( "*.*", path =>
		{
			// Prevent an infinite loop -- don't recompile assets that we just compiled!
			if ( path.EndsWith( "_c" ) )
				return;

			// Even though FileSystemWatcher has raised an event, it does not mean that
			// a lock has been released. We will wait a short while to see if the file
			// is still locked before we attempt to compile it.
			{
				const int RetryCount = 3;

				for ( int i = 0; i < RetryCount; ++i )
				{
					if ( FileSystem.Mounted.IsFileReady( path, FileSystemOptions.AssetCompiler ) )
						break;

					Thread.Sleep( 500 );
				}
			}

			CompileFile( path, true );
		} );
	}
}
