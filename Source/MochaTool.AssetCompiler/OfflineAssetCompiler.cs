global using Mocha.Common;

namespace MochaTool.AssetCompiler;

public class OfflineAssetCompiler : AssetCompilerBase
{
	public void Run( Options options )
	{
		var start = DateTime.Now;

		Log = new ConsoleLogger();

		FileSystem.Mounted = new( options.MountPoints.ToArray() );
		List<string> queue = new();

		// Target is directory
		QueueDirectory( ref queue, "." );

		var dispatcher = new ThreadDispatcher<string>( async ( threadQueue ) =>
		{
			var tasks = new List<Task>();

			foreach ( var relativePath in threadQueue )
			{
				try
				{
					tasks.Add( CompileFileAsync( relativePath ) );
				}
				catch ( Exception e )
				{
					ResultLog.Fail( relativePath, e );
				}
			}

			await Task.WhenAll( tasks );
		}, queue );

		while ( !dispatcher.IsComplete )
			Thread.Sleep( 5 );

		ResultLog.Results( (DateTime.Now - start) );
	}

	private void QueueDirectory( ref List<string> queue, string directory )
	{
		foreach ( var file in FileSystem.Mounted.GetFilesAbsolute( directory, FileSystemOptions.AssetCompiler ) )
		{
			QueueFile( ref queue, file );
		}

		foreach ( var subDirectory in FileSystem.Mounted.GetDirectoriesAbsolute( directory, FileSystemOptions.AssetCompiler ) )
		{
			QueueDirectory( ref queue, subDirectory );
		}
	}

	private void QueueFile( ref List<string> queue, string path )
	{
		var fileExtension = Path.GetExtension( path );

		if ( TryGetCompiler( fileExtension, out var _ ) )
		{
			queue.Add( path );
		}
		else
		{
			ResultLog.UnknownType( path );
		}
	}
}
