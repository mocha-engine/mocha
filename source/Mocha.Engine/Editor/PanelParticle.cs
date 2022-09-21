using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class PanelParticle : Panel
{
	private Vector2 velocity;

	public PanelParticle( Vector2 velocity, Vector4 color, Rectangle rect ) : base( rect )
	{
		this.velocity = velocity;
		this.color = color;
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		base.Render( ref panelRenderer );

		velocity -= new Vector2( 0, 128 ) * Time.Delta;

		rect.X -= velocity.X * Time.Delta;
		rect.Y -= velocity.Y * Time.Delta;
	}
}
