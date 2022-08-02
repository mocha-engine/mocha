namespace Mocha.InteropGen;

public class Program
{
	private static void ProcessHeader( string headerPath )
	{
		Console.WriteLine( $"-- Processing header {headerPath}" );

		var fileContents = File.ReadAllText( headerPath );
		var headerParser = new HeaderParser( fileContents );
	}

	private static void ProcessDirectory( string directoryPath )
	{
		foreach ( var file in Directory.GetFiles( directoryPath ) )
		{
			if ( file.EndsWith( ".h" ) )
			{
				ProcessHeader( file );
			}
		}

		foreach ( var subDirectory in Directory.GetDirectories( directoryPath ) )
		{
			ProcessDirectory( subDirectory );
		}
	}

	public static void Main()
	{
		ProcessDirectory( "." );
	}
}
