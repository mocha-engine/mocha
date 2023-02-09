namespace Mocha.Common;

public static class Core
{
	public static bool IsServer => Engine.IsServer();
	public static bool IsClient => Engine.IsClient();
	public static int TickRate { get; set; }
}
