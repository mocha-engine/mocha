namespace Mocha.Common;

public interface ILogger
{
	Action<string> OnLog { get; set; }

	List<NativeLogger.LogEntry> GetHistory();
	void Error( object? obj );
	void Info( object? obj );
	void Trace( object? obj );
	void Warning( object? obj );
}
