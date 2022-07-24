using Veldrid;

namespace Mocha.Engine;

internal class BaseTab
{
	public ImGuiRenderer ImGuiRenderer { get; set; }

	public bool isVisible = false;

	public virtual void Draw()
	{
	}
}
