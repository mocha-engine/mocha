namespace Mocha.Common;

public struct ConsoleMessage
{
	public uint Color { get; set; }
	public string Message { get; set; }
	public string CallingClass { get; set; }
	public string[] StackTrace { get; set; }
}
