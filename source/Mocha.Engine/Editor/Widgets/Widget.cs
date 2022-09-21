using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Widget
{
	public Rectangle Bounds { get; set; }
	public int ZIndex { get; set; } = 0;

	internal Widget()
	{
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
	}

	internal virtual Vector2 GetDesiredSize()
	{
		return Bounds.Size;
	}
}
