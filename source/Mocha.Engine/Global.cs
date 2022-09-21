global using Mocha.Common;
global using Mocha.Renderer;
global using Mocha.Engine.Editor;
global using System.ComponentModel;

namespace Mocha.Engine;

public static class Global
{
	public static Logger Log { get; } = new();
}
