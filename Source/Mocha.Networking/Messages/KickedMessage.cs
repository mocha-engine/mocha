namespace Mocha.Networking;

public class KickedMessage : IBaseNetworkMessage
{
	public string Reason { get; set; } = "Kicked";
}
