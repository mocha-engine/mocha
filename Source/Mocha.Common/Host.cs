namespace Mocha.Common;

public static class Host
{
	public static bool IsServer => Glue.Engine.IsServer();
	public static bool IsClient => Glue.Engine.IsClient();
}
