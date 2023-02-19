using Mocha.Common;

namespace Mocha.Networking;

public class ClientInputMessage : BaseNetworkMessage
{
	public short LerpMsec { get; set; }
	public byte Msec { get; set; }
	public Vector3 ViewAngles { get; set; }
	public float ForwardMove { get; set; }
	public float SideMove { get; set; }
	public float UpMove { get; set; }
	public ushort Buttons { get; set; }
}
