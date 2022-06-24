namespace Mocha.Common;

public struct ConsoleMessage
{
	public uint Color { get; set; }
	public string Message { get; set; }
	public string CallingClass { get; set; }
	public string[] StackTrace { get; set; }

	public static ConsoleMessage CreateGeneric( string message )
	{
		return new ConsoleMessage()
		{
			Message = message,
			Color = 0xFFFFFFFF,
			CallingClass = "Generic",
			StackTrace = new[] { "" }
		};
	}
}
