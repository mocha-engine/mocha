namespace Mocha.InteropGen;

public class FileWriter : IWriter
{
	private StreamWriter streamWriter;

	public FileWriter( string filePath )
	{
		if ( File.Exists( filePath ) )
			File.Delete( filePath );

		streamWriter = new( filePath );
	}

	public void Dispose()
	{
		streamWriter.Dispose();
	}

	public void Write( string str )
	{
		streamWriter.Write( str );
	}

	public void WriteLine( string str )
	{
		streamWriter.WriteLine( str );
	}

	public void WriteLine()
	{
		streamWriter.WriteLine();
	}
}
