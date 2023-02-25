namespace Mocha.Networking;

public class ClientInputMessage : IBaseNetworkMessage
{
	[Replicated] public bool Left { get; set; }
	[Replicated] public bool Right { get; set; }
	[Replicated] public bool Middle { get; set; }

	[Replicated] public float ViewAnglesP { get; set; }
	[Replicated] public float ViewAnglesY { get; set; }
	[Replicated] public float ViewAnglesR { get; set; }

	[Replicated] public float DirectionX { get; set; }
	[Replicated] public float DirectionY { get; set; }
	[Replicated] public float DirectionZ { get; set; }
}
