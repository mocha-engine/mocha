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

	private void Log( object? obj, Action<string, string> logAction )
	{
		var loggerName = Core.IsClient ? "cl" : "sv";

		string str = GetString( obj );
		OnLog?.Invoke( str );
		logAction( loggerName, str );
	}

	public void Trace( object? obj ) => Log( obj, NativeEngine.GetLogManager().ManagedTrace );
	public void Info( object? obj ) => Log( obj, NativeEngine.GetLogManager().ManagedInfo );
	public void Warning( object? obj ) => Log( obj, NativeEngine.GetLogManager().ManagedWarning );
	public void Error( object? obj ) => Log( obj, NativeEngine.GetLogManager().ManagedError );

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
		var logManager = NativeEngine.GetLogManager();
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
