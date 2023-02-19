using Mocha.Common;

namespace Mocha.Networking;

public class BaseNetworkMessage
{
	public virtual byte[] Serialize()
	{
		return Serializer.Serialize( this );
	}
}
