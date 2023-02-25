using Mocha.Common;

namespace Mocha.Networking;

public class HandshakeMessage : IBaseNetworkMessage
{
	[Replicated] public int TickRate { get; set; }
	[Replicated] public string? Nickname { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
