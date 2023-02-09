using System.Runtime.InteropServices;

namespace Mocha.Common;

public class NativeLogger : ILogger
{
	public Action<string> OnLog { get; set; }

	private string GetString( object? obj )
	{
		string logStr = "";
		if ( obj != null )
			logStr = obj.ToString()!;

		return logStr;
	}

	public void Trace( object? obj )
	{
		string str = GetString( obj );
		OnLog?.Invoke( str );
		Glue.LogManager.ManagedTrace( str );
	}

	public void Info( object? obj )
	{
		string str = GetString( obj );
		OnLog?.Invoke( str );
		Glue.LogManager.ManagedInfo( str );
	}

	public void Warning( object? obj )
	{
		string str = GetString( obj );
		OnLog?.Invoke( str );
		Glue.LogManager.ManagedWarning( str );
	}

	public void Error( object? obj )
	{
		string str = GetString( obj );
		OnLog?.Invoke( str );
		Glue.LogManager.ManagedError( str );
	}

	public struct LogEntry
	{
		[MarshalAs( UnmanagedType.LPStr )]
		public string time;

		[MarshalAs( UnmanagedType.LPStr )]
		public string logger;

		[MarshalAs( UnmanagedType.LPStr )]
		public string level;

		[MarshalAs( UnmanagedType.LPStr )]
		public string message;
	}

	public List<LogEntry> GetHistory()
	{
		var logManager = Engine.GetLogManager();
		var logHistory = logManager.GetLogHistory();

		LogEntry[] logEntries = new LogEntry[logHistory.count];
		var ptr = logHistory.items;

		for ( int i = 0; i < logHistory.count; ++i )
		{
			logEntries[i] = Marshal.PtrToStructure<LogEntry>( ptr );
			ptr += Marshal.SizeOf( logEntries[i] );
		}

		return logEntries.ToList();
	}
}
