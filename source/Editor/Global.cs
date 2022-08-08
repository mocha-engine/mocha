global using Mocha.Common;
global using Mocha.Renderer;

namespace Mocha.Editor;

public static class Global
{
	public static Logger Log { get; } = new();
}
