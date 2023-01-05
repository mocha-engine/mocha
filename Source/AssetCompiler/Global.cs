global using static AssetCompiler.Global;

namespace AssetCompiler;

public static class Global
{
	public static Logger Log { get; set; } = new();
}
