using Mocha.Common;

namespace Mocha.Networking;

public class HandshakeMessage : IBaseNetworkMessage
{
	public int TickRate { get; set; }
	public string? Nickname { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
