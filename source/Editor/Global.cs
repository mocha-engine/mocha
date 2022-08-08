global using Mocha.Common;
global using Mocha.Engine.Editor;
global using Mocha.Renderer;
global using System.ComponentModel;

namespace Mocha.Editor;

public static class Global
{
	public static Logger Log { get; } = new();
}
