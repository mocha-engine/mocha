using MessagePack;

namespace Mocha.Networking;

[MessagePackObject]
public class KickedMessage : IBaseNetworkMessage
{
	[IgnoreMember]
	public MessageID MessageID => MessageID.Kicked;

	[Key( 0 )] public string Reason { get; set; } = "Kicked";
}
