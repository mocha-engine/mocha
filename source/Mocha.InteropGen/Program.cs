namespace Mocha.InteropGen;

public class Program
{
	private static void ProcessHeader( string baseDir, string headerPath )
	{
		Console.WriteLine( $"\t Processing header {headerPath}" );

		var fileContents = File.ReadAllText( headerPath );
		var headerParser = new HeaderParser( baseDir, headerPath, fileContents );
	}

	private static void ProcessDirectory( string baseDir, string directoryPath )
	{
		foreach ( var file in Directory.GetFiles( directoryPath ) )
		{
			if ( file.EndsWith( ".h" ) && !file.EndsWith( ".generated.h" ) )
			{
				ProcessHeader( baseDir, file );
			}
		}

		foreach ( var subDirectory in Directory.GetDirectories( directoryPath ) )
		{
			ProcessDirectory( baseDir, subDirectory );
		}
	}

	public static void Main( string[] args )
	{
		Console.WriteLine( "Generating C# <--> C++ interop code..." );
		ProcessDirectory( args[0], args[0] );
	}
}
