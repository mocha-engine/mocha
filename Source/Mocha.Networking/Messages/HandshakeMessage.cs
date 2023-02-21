using Mocha.Common;

namespace Mocha.Networking;

public class HandshakeMessage : IBaseNetworkMessage
{
	public static int MessageId => 0;
	public int TickRate { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
