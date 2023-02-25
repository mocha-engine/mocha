namespace Mocha.Networking;

public class KickedMessage : IBaseNetworkMessage
{
	[Replicated] public string Reason { get; set; } = "Kicked";
}
