using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Widget
{
	public Rectangle Bounds;

	internal Widget( Rectangle bounds )
	{
		this.Bounds = bounds;
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
	}
}
