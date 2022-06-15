global using System.Numerics;

namespace Mocha.AssetCompiler;

public static class Program
{
	public static void Main( string[] args )
	{
		if ( args.Length == 0 )
		{
			Console.WriteLine( "Expected filename" );
			return;
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

	private static void QueueFile( ref List<string> queue, string fileName )
	{
		var fileExtension = Path.GetExtension( fileName );

		if ( fileExtension == ".fbx" )
		{
			queue.Add( fileName );
		}
		else if ( fileExtension == ".png" )
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

		if ( fileExtension == ".fbx" )
		{
			// Check if we have an original asset & if it needs recompiling
			var destFile = ModelCompiler.CompileFile( fileName );
			Console.WriteLine( $"[MODEL OK]\t{destFile}" );
		}
		else if ( fileExtension == ".png" )
		{
			var destFile = TextureCompiler.CompileFile( fileName );
			Console.WriteLine( $"[TEXTURE OK]\t{destFile}" );
		}
	}
}
