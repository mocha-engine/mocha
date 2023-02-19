namespace Mocha.Networking;

public class KickedMessage : BaseNetworkMessage
{
	public string Reason { get; set; } = "Kicked";
}
