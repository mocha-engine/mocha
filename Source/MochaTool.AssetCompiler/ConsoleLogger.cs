namespace MochaTool.AssetCompiler;

public class ConsoleLogger : ILogger
{
	public Action<string> OnLog { get; set; }

	public void Error( object? obj )
	{
		Console.WriteLine( "[ERROR]		" + (obj?.ToString() ?? "null") );
	}

	public void Info( object? obj )
	{
		Console.WriteLine( "[INFO]		" + (obj?.ToString() ?? "null") );
	}

	public void Trace( object? obj )
	{
		Console.WriteLine( "[TRACE]		" + (obj?.ToString() ?? "null") );
	}

	public void Warning( object? obj )
	{
		Console.WriteLine( "[WARNING]	" + (obj?.ToString() ?? "null") );
	}

	public List<NativeLogger.LogEntry> GetHistory()
	{
		return new();
	}
}
