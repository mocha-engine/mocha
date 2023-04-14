using MessagePack;

namespace Mocha.Networking;

[MessagePackObject]
public class NetworkMessageWrapper
{
	[Key( 0 )] public MessageID? Type { get; set; }
	[Key( 1 )] public byte[]? Data { get; set; }
}
