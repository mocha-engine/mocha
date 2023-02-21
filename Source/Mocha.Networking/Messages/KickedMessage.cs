namespace Mocha.Networking;

public class KickedMessage : IBaseNetworkMessage
{
	public static int MessageId => 2;
	public string Reason { get; set; } = "Kicked";
}
