global using Mocha.Common;
using System.Reflection;

namespace Mocha.AssetCompiler;

public static class Program
{
	private static List<BaseCompiler> Compilers = new();

	public static void Main( string[] args )
	{
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
		int threadCount = 8;

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

		var batchSize = queue.Count / threadCount - 1;
		var batched = queue
			.Select( ( Value, Index ) => new { Value, Index } )
			.GroupBy( p => p.Index / batchSize )
			.Select( g => g.Select( p => p.Value ).ToList() )
			.ToList();

		for ( int i = 0; i < batched.Count; i++ )
		{
			var threadQueue = batched[i];
			var thread = new Thread( () =>
			 {
				 foreach ( var item in threadQueue )
				 {
					 CompileFile( item );
				 }
			 } );
			thread.Start();
		}
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

	private static bool GetCompiler( string fileExtension, out BaseCompiler? foundCompiler )
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

	private static void QueueFile( ref List<string> queue, string fileName )
	{
		var fileExtension = Path.GetExtension( fileName );

		if ( GetCompiler( fileExtension, out var _ ) )
		{
			queue.Add( fileName );
		}
		else
		{
			Console.WriteLine( $"[SKIP]\t'{fileExtension}' for path '{fileName}'..." );
		}
	}

	private static void CompileFile( string fileName )
	{
		var fileExtension = Path.GetExtension( fileName );

		// TODO: Check if we have an original asset & if it needs recompiling

		if ( GetCompiler( fileExtension, out var compiler ) )
		{
			var destFile = compiler.CompileFile( fileName );
			Console.WriteLine( $"[OK]\t\t{destFile}" );
		}
	}
}
