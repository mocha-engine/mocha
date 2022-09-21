using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Panel : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );

	internal Panel( Common.Rectangle rect ) : base( rect )
	{
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
		panelRenderer.AddRectangle( Bounds, Color );
	}
}
