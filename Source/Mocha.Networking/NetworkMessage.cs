namespace Mocha.Networking;

public class NetworkMessageWrapper
{
	[Replicated] public string? Type { get; set; }
	[Replicated] public byte[]? Data { get; set; }
}
