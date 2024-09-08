using MessagePack;

namespace Mocha.Networking;

[MessagePackObject( true )]
public class KickedMessage : IBaseNetworkMessage
{
	[IgnoreMember]
	public MessageID MessageID => MessageID.Kicked;

	public string Reason { get; set; } = "Kicked";
}
