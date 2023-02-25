using MessagePack;

namespace Mocha.Networking;

[MessagePackObject]
public class KickedMessage : IBaseNetworkMessage
{
	[Key( 0 )] public string Reason { get; set; } = "Kicked";
}
