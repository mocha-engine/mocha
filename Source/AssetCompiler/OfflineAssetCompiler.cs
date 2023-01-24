global using Mocha.Common;

namespace MochaTool.AssetCompiler;

public class OfflineAssetCompiler : AssetCompilerBase
{
	public void Run( Options options )
	{
		var start = DateTime.Now;

		if ( !Path.Exists( options.Target ) )
		{
			Console.WriteLine( $"'{options.Target}' is not a valid target." );
			return;
		}

		var attr = File.GetAttributes( options.Target );

		List<string> queue = new();

		if ( attr.HasFlag( FileAttributes.Directory ) )
		{
			// Target is directory
			QueueDirectory( ref queue, options.Target );

			var dispatcher = new ThreadDispatcher<string>( async ( threadQueue ) =>
			{
				var tasks = new List<Task>();

				foreach ( var item in threadQueue )
				{
					try
					{
						tasks.Add( CompileFileAsync( item ) );
					}
					catch ( Exception e )
					{
						Log.Fail( item, e );
					}
				}

				await Task.WhenAll( tasks );
			}, queue );

			while ( !dispatcher.IsComplete )
				Thread.Sleep( 5 );
		}
		else
		{
			// Target is single file
			CompileFile( options.Target );
		}

		Log.Results( (DateTime.Now - start) );
	}

	private void QueueDirectory( ref List<string> queue, string directory )
	{
		foreach ( var file in Directory.GetFiles( directory ) )
		{
			QueueFile( ref queue, file );
		}

		foreach ( var subDirectory in Directory.GetDirectories( directory ) )
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
			Log.UnknownType( path );
		}
	}
}
