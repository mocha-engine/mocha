using Mocha.Renderer.UI;

namespace Mocha.Engine;

internal class Label : Panel
{
	public string Text { get; set; }
	public float FontSize { get; set; } = 16f;

	internal Label( string text, Common.Rectangle rect, float fontSize = 16f ) : base( rect )
	{
		FontSize = fontSize;
		Text = text;
	}

	private Rectangle FontBoundsToAtlasRect( Glyph glyph, Bounds bounds )
	{
		Vector2 min = new Vector2( glyph.AtlasBounds.Left,
								  glyph.AtlasBounds.Top );

		Vector2 max = new Vector2( glyph.AtlasBounds.Right,
								  glyph.AtlasBounds.Bottom );

		Vector2 mins = min;
		Vector2 maxs = (max - min) * new Vector2( 1, -1 );

		var glyphRect = new Rectangle( mins, maxs );

		glyphRect /= Editor.Atlas.Size;
		glyphRect.Y = 1.0f - glyphRect.Y;

		return glyphRect;
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		float x = 0;

		foreach ( var c in Text )
		{
			var glyph = Editor.FontData.Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
				glyphSize *= FontSize * 6;

				var glyphPos = new Rectangle( new Vector2( rect.X + x, rect.Y + FontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * FontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * FontSize;

				panelRenderer.AddRectangle(
					glyphPos,
					glyphRect,
					color
				);
			}

			x += (float)glyph.Advance * FontSize;
		}
	}
}
