namespace Mocha.Networking;

public class NetworkMessage<T> where T : BaseNetworkMessage
{
	public int NetworkMessageType { get; set; }
	public T Data { get; set; } = null!;
}
