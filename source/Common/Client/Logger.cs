using System.Diagnostics;

namespace Mocha.Common;

public class Logger
{
	public Glue.CLogger NativeLogger { get; set; }

	public enum Level
	{
		Trace,
		Info,
		Warning,
		Error
	};

	public void Trace( object obj ) => NativeLogger.Trace( obj?.ToString() );
	public void Info( object obj ) => NativeLogger.Info( obj?.ToString() );
	public void Warning( object obj ) => NativeLogger.Warning( obj?.ToString() );
	public void Error( object obj ) => NativeLogger.Error( obj?.ToString() );

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
