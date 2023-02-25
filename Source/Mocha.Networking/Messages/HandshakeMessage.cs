using MessagePack;
using Mocha.Common;

namespace Mocha.Networking;

[MessagePackObject]
public class HandshakeMessage : IBaseNetworkMessage
{
	[Key( 0 )] public int TickRate { get; set; }
	[Key( 1 )] public string? Nickname { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
