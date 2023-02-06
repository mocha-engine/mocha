namespace Mocha.Common;

public static class Core
{
	public static bool IsServer => Glue.Engine.IsServer();
	public static bool IsClient => Glue.Engine.IsClient();
	public static int TickRate { get; set; }
}
