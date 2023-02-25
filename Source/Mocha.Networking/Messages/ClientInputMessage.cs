using MessagePack;

namespace Mocha.Networking;

[MessagePackObject]
public class ClientInputMessage : IBaseNetworkMessage
{
	[Key( 0 )] public bool Left { get; set; }
	[Key( 1 )] public bool Right { get; set; }
	[Key( 2 )] public bool Middle { get; set; }

	[Key( 3 )] public float ViewAnglesP { get; set; }
	[Key( 4 )] public float ViewAnglesY { get; set; }
	[Key( 5 )] public float ViewAnglesR { get; set; }

	[Key( 6 )] public float DirectionX { get; set; }
	[Key( 7 )] public float DirectionY { get; set; }
	[Key( 8 )] public float DirectionZ { get; set; }
}
