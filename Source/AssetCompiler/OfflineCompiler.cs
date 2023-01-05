global using Mocha.Common;
using System.Reflection;

namespace Mocha.AssetCompiler;

public class OfflineCompiler
{
	public static OfflineCompiler Current { get; private set; }
	private List<BaseCompiler> Compilers = new();

	public OfflineCompiler()
	{
		Current = this;

		foreach ( var type in Assembly.GetExecutingAssembly().GetTypes().Where( x => x.BaseType == typeof( BaseCompiler ) ) )
		{
			var instance = Activator.CreateInstance( type ) as BaseCompiler;

			if ( instance != null )
				Compilers.Add( instance );
		}
	}

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

			var dispatcher = new ThreadDispatcher<string>( ( threadQueue ) =>
			{
				foreach ( var item in threadQueue )
				{
					CompileFile( item );
				}
			}, queue );

			while ( !dispatcher.IsComplete )
				Thread.Sleep( 500 );
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

	private bool GetCompiler( string fileExtension, out BaseCompiler? foundCompiler )
	{
		foreach ( var compiler in Compilers )
		{
			if ( compiler.GetType().GetCustomAttribute<HandlesAttribute>()?.Extensions?.Contains( fileExtension ) ?? false )
			{
				foundCompiler = compiler;
				return true;
			}
		}

		foundCompiler = null;
		return false;
	}

	private void QueueFile( ref List<string> queue, string path )
	{
		var fileExtension = Path.GetExtension( path );

		if ( GetCompiler( fileExtension, out var _ ) )
		{
			queue.Add( path );
		}
		else
		{
			Log.UnknownType( path );
		}
	}

	public void CompileFile( string path )
	{
		var fileExtension = Path.GetExtension( path );

		// TODO: Check if we have an original asset & if it needs recompiling

		if ( GetCompiler( fileExtension, out var compiler ) )
		{
			var destFile = compiler?.CompileFile( path );

			if ( destFile == null )
				throw new Exception( "Failed to compile?" );

			Log.Compiled( destFile );
		}
	}
}
