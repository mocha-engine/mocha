﻿using System.Runtime.InteropServices;

namespace Mocha.Common;

public class Logger
{
	public void Trace( object obj ) => Glue.LogManager.ManagedTrace( obj?.ToString() );
	public void Info( object obj ) => Glue.LogManager.ManagedInfo( obj?.ToString() );
	public void Warning( object obj ) => Glue.LogManager.ManagedWarning( obj?.ToString() );
	public void Error( object obj ) => Glue.LogManager.ManagedError( obj?.ToString() );

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
		var logHistory = Glue.LogManager.GetLogHistory();

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
