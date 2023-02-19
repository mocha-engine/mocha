using Mocha.Common;

namespace Mocha.Networking;

public class NetworkMessageWrapper<T> where T : BaseNetworkMessage
{
	public int NetworkMessageType { get; set; } = -1;
	public T Data { get; set; } = null!;

	public virtual byte[] Serialize()
	{
		return Serializer.Serialize( this );
	}
}
