using Mocha.Common;

namespace Mocha.Networking;

public class HandshakeMessage : BaseNetworkMessage
{
	public int TickRate { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
