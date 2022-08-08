namespace Mocha.InteropGen;

public class ConsoleWriter : IWriter
{
	public ConsoleWriter( string filePath )
	{
		Console.WriteLine( filePath + ":" );
		Console.ForegroundColor = ConsoleColor.Cyan;
	}

	public void Dispose()
	{
		Console.ForegroundColor = ConsoleColor.Gray;
	}

	public void Write( string str )
	{
		Console.Write( str );
	}

	public void WriteLine( string str )
	{
		Console.WriteLine( str );
	}

	public void WriteLine()
	{
		Console.WriteLine();
	}
}
