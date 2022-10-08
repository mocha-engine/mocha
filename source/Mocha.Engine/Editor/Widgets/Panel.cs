using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

[Flags]
public enum PanelInputFlags
{
	None		= 0,
	MouseOver	= 1,
	MouseDown	= 2
}

internal class Panel : Widget
{
	public Vector4 Color { get; set; } = new Vector4( 1, 1, 1, 1 );

	internal Panel( Vector2 size ) : base()
	{
		Bounds = new Rectangle( 0, size );
	}

	internal virtual void Render( ref PanelRenderer panelRenderer )
	{
		Graphics.DrawRect( Bounds, Color );
	}
}
