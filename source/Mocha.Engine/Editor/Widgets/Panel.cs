using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Panel
{
	public Vector4 color = new Vector4( 1, 1, 1, 1 );
	public Common.Rectangle rect;

	internal Panel( Common.Rectangle rect )
	{
		this.rect = rect;
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
		panelRenderer.AddRectangle( rect, color );
	}
}
