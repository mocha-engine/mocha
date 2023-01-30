global using static AssetCompiler.Global;
global using static Mocha.Common.Global;

namespace AssetCompiler;

public static class Global
{
	public static ResultLogger ResultLog { get; set; } = new();
}
