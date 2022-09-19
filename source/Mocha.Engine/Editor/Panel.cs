using Mocha.Renderer.UI;

namespace Mocha.Engine;

internal class Panel
{
	protected Vector3 color = new Vector3( 0.5f, 1.0f, 0.5f );
	public Common.Rectangle rect;

	internal Panel( Common.Rectangle rect )
	{
		this.rect = rect;
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
		var col = color;

		if ( rect.Contains( Input.MousePosition ) )
		{
			col *= 3f;

			if ( Input.MouseLeft )
				col *= 0.1f;
		}

		panelRenderer.AddRectangle( rect, col );
	}
}
