namespace Mocha.Networking;

public class NetworkMessageWrapper
{
	public int NetworkMessageType { get; set; } = -1;
	public byte[]? Data { get; set; }
}
