namespace Mocha.Common;

public struct ConsolePacket<T>
{
	public int ProtocolVersion { get; set; }
	public int DataSize { get; set; }
	public T Data { get; set; }
}
