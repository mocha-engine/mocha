global using Mocha.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.AssetCompiler;

public static class Program
{
	private static List<BaseCompiler> Compilers = new();

	public static void Main( string[] args )
	{
		var start = DateTime.Now;

		if ( args.Length == 0 )
		{
			Console.WriteLine( "Expected filename" );
			return;
		}

		foreach ( var type in Assembly.GetExecutingAssembly().GetTypes().Where( x => x.BaseType == typeof( BaseCompiler ) ) )
		{
			var instance = Activator.CreateInstance( type ) as BaseCompiler;

			if ( instance != null )
				Compilers.Add( instance );
		}

		var path = args[0];
		var attr = File.GetAttributes( path );

		List<string> queue = new();

		if ( attr.HasFlag( FileAttributes.Directory ) )
		{
			// Directory
			QueueDirectory( ref queue, path );
		}
		else
		{
			// Single file
			CompileFile( path );
		}

		var dispatcher = new ThreadDispatcher<string>( ( threadQueue ) =>
		{
			foreach ( var item in threadQueue )
			{
				CompileFile( item );
			}
		}, queue );

		while ( !dispatcher.IsComplete )
			Thread.Sleep( 500 );

		Log.Results( (DateTime.Now - start) );
	}

	private static void QueueDirectory( ref List<string> queue, string directory )
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

	private static bool GetCompiler( string fileExtension, [NotNullWhen( true )] out BaseCompiler? foundCompiler )
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

	private static void QueueFile( ref List<string> queue, string path )
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

	public static void CompileFile( string path )
	{
		var fileExtension = Path.GetExtension( path );

		// TODO: Check if we have an original asset & if it needs recompiling

		if ( !GetCompiler( fileExtension, out var compiler ) )
			return;

		Log.Processing( compiler.AssetName, path );
		var result = compiler.CompileFile( path );

		switch ( result.State )
		{
			case CompileState.UpToDate:
				Log.UpToDate( result.DestinationPath! );
				break;
			case CompileState.Succeeded:
				Log.Compiled( result.DestinationPath! );
				break;
			case CompileState.Failed:
				throw new Exception( "Failed to compile?" );
			default:
				throw new UnreachableException();
		}
	}
}
