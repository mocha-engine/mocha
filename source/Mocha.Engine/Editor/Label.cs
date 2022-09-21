using Mocha.Renderer.UI;

namespace Mocha.Engine;

internal class Label : Panel
{
	public string Text { get; set; }
	public float FontSize { get; set; } = 16f;

	[Obsolete( "Bad place for this" )]
	public static Vector2 MeasureText( string text, float fontSize )
	{
		float x = 0;

		foreach ( var c in text )
		{
			var glyph = Editor.FontData.Glyphs.First( x => x.Unicode == c );
			x += (float)glyph.Advance * fontSize;
		}

		return new Vector2( x, 0 );
	}

	internal Label( string text, Common.Rectangle rect, float fontSize = 16f ) : base( rect )
	{
		color = ITheme.Current.TextColor;
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

		glyphRect /= Editor.AtlasTexture.Size;
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

				float heightMul = Editor.AtlasTexture.Height / Editor.FontSprite.Rect.Height;

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height * heightMul );
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
