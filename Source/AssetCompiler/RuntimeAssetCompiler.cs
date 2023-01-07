namespace Mocha.AssetCompiler;

public class RuntimeAssetCompiler : AssetCompilerBase
{
	private FileSystemWatcher Watcher { get; set; }

	public RuntimeAssetCompiler()
	{
		// Only notify on file created
		var filters = NotifyFilters.CreationTime;

		Watcher = FileSystem.Game.CreateWatcher( "", "*.*", path =>
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
					if ( FileSystem.Game.IsFileReady( path ) )
						break;

					Thread.Sleep( 500 );
				}
			}

			CompileFile( path );
		}, filters );
	}
}
