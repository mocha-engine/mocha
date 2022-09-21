using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class RoundedPanel : Panel
{
	public float Radius { get; set; } = 16f;

	public RoundedPanel( Rectangle rect, float radius = 16f ) : base( rect )
	{
		Radius = radius;
	}

	private void DrawSegment( ref PanelRenderer panelRenderer, Rectangle offset, Vector2 corner, Vector2? _scale = null )
	{
		var scale = _scale ?? new Vector2( 1f, 1f );

		var ndcRect = Editor.SDFSprite.Rect;
		ndcRect += corner * ndcRect.Size * 0.5f;
		ndcRect /= Editor.AtlasTexture.Size;

		ndcRect.Width /= 2.0f;
		ndcRect.Height /= 2.0f;

		ndcRect.Width *= scale.X;
		ndcRect.Height *= scale.Y;

		var topLeftRect = rect;
		topLeftRect.Width = offset.Width;
		topLeftRect.Height = offset.Height;

		topLeftRect.X += offset.X;
		topLeftRect.Y += offset.Y;

		panelRenderer.AddRectangle( topLeftRect, ndcRect, -1, color );
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		//var max = rect.Size - Radius;

		//// Top left
		//DrawSegment( ref panelRenderer, 
		//	 new Rectangle( 0, 0, Radius, Radius ), 
		//	  new Vector2( 0, 0 ) 
		//);

		//// Bottom left
		//DrawSegment( ref panelRenderer,
		//	 new Rectangle( 0, max.Y, Radius, Radius ),
		//	  new Vector2( 0, 1 )
		//);

		//// Top right
		//DrawSegment( ref panelRenderer,
		//	 new Rectangle( max.X, 0, Radius, Radius ),
		//	  new Vector2( 1, 0 )
		//);

		//// Bottom right
		//DrawSegment( ref panelRenderer,
		//	 new Rectangle( max.X, max.Y, Radius, Radius ),
		//	  new Vector2( 1, 1 )
		//);

		//// Center
		//var centerRect = rect;
		//centerRect.X += Radius;
		//centerRect.Width -= Radius * 2.0f;
		//centerRect.Y += 3.5f;
		//centerRect.Height -= 5f;
		//panelRenderer.AddRectangle( centerRect, color );

		//// Middle
		//var middleLeftRect = rect;
		//middleLeftRect.Y += Radius;
		//middleLeftRect.Height -= Radius * 2.0f;
		//middleLeftRect.X += 3.5f;
		//middleLeftRect.Width -= 5f;
		//panelRenderer.AddRectangle( middleLeftRect, color );

		panelRenderer.AddRoundedRectangle( rect, Radius, color );
	}
}
