using Veldrid;

namespace Mocha.Engine;

internal class BaseTab
{
	public ImGuiRenderer ImGuiRenderer { get; set; }

	public bool visible = false;

	public virtual void Draw()
	{
	}
}
