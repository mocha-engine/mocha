namespace Mocha.Networking;

public class ClientInputMessage : IBaseNetworkMessage
{
	public bool Left { get; set; }
	public bool Right { get; set; }
	public bool Middle { get; set; }

	public float ViewAnglesP { get; set; }
	public float ViewAnglesY { get; set; }
	public float ViewAnglesR { get; set; }

	public float DirectionX { get; set; }
	public float DirectionY { get; set; }
	public float DirectionZ { get; set; }
}
