namespace Mocha.Common;

public static class Core
{
	public static bool IsServer => NativeEngine.IsServer();
	public static bool IsClient => NativeEngine.IsClient();
	public static int TickRate { get; set; }
}
