using MessagePack;
using Mocha.Common;

namespace Mocha.Networking;

[MessagePackObject( true )]
public class HandshakeMessage : IBaseNetworkMessage
{
	[IgnoreMember]
	public MessageID MessageID => MessageID.Handshake;

	public float Timestamp { get; set; }
	public int TickRate { get; set; }
	public string? Nickname { get; set; }

	public HandshakeMessage()
	{
		TickRate = Core.TickRate;
	}
}
