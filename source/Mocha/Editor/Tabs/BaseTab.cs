using Veldrid;

namespace Mocha;

internal class BaseTab
{
	public ImGuiRenderer ImGuiRenderer { get; set; }

	public bool visible = false;

	public virtual void Draw()
	{
	}
}
