namespace Mocha.Common;

public struct ConsolePacket
{
	public int ProtocolVersion { get; set; }
	public string Identifier { get; set; }
	public int DataSize { get; set; }
	public byte[] Data { get; set; }
}
