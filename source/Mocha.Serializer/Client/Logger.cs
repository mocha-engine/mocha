using System.Diagnostics;

namespace Mocha.Common;

public class Logger
{
	public enum Level
	{
		Trace,
		Info,
		Warning,
		Error
	};

	public void Trace( object obj ) => InternalLog( obj?.ToString(), Level.Trace );
	public void Info( object obj ) => InternalLog( obj?.ToString(), Level.Info );
	public void Warning( object obj ) => InternalLog( obj?.ToString(), Level.Warning );
	public void Error( object obj ) => InternalLog( obj?.ToString(), Level.Error );

	public static Action<Level, string, StackTrace> OnLog;

	private static void InternalLog( string? str, Level severity = Level.Trace )
	{
		if ( str == null )
			return;

		var stackTrace = new System.Diagnostics.StackTrace();

#if RELEASE
		if ( severity == Level.Error )
			throw new Exception( str );
#endif

		Console.Write( $"[{DateTime.Now.ToLongTimeString()}] " );

		Console.ForegroundColor = SeverityToConsoleColor( severity );
		Console.Write( $"[{severity}] ".Pad() );
		Console.ForegroundColor = ConsoleColor.Gray;

		Console.WriteLine( $"{str}" );

		OnLog?.Invoke( severity, str, stackTrace );
	}

	private static ConsoleColor SeverityToConsoleColor( Level severity )
	{
		switch ( severity )
		{
			case Level.Error:
				return ConsoleColor.DarkRed;
			case Level.Warning:
				return ConsoleColor.Red;
			case Level.Trace:
				return ConsoleColor.DarkGray;
			case Level.Info:
				return ConsoleColor.White;
		}

		return ConsoleColor.White;
	}
}
