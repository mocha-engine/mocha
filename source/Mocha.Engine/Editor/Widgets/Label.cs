using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

internal class Label : Widget
{
	public string Text { get; set; }
	public float FontSize { get; set; } = 16f;

	[Obsolete( "Bad place for this" )]
	public static Vector2 MeasureText( string text, float fontSize )
	{
		float x = 0;

		foreach ( var c in text )
		{
			var glyph = EditorInstance.FontData.Glyphs.First( x => x.Unicode == c );
			x += (float)glyph.Advance * fontSize;
		}

		return new Vector2( x, fontSize * 1.25f );
	}

	internal Label( string text, float fontSize = 16f )
	{
		FontSize = fontSize;
		Text = text;
	}

	private Rectangle FontBoundsToAtlasRect( Font.Glyph glyph, Font.Bounds bounds )
	{
		Vector2 min = new Vector2( glyph.AtlasBounds.Left,
								  glyph.AtlasBounds.Top );

		Vector2 max = new Vector2( glyph.AtlasBounds.Right,
								  glyph.AtlasBounds.Bottom );

		Vector2 mins = min;
		Vector2 maxs = (max - min) * new Vector2( 1, -1 );

		var glyphRect = new Rectangle( mins, maxs );

		glyphRect /= EditorInstance.AtlasTexture.Size;
		glyphRect.Y = 1.0f - glyphRect.Y;

		return glyphRect;
	}

	internal override void Render( ref PanelRenderer panelRenderer )
	{
		float x = 0;

		foreach ( var c in Text )
		{
			var glyph = EditorInstance.FontData.Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

				float heightMul = EditorInstance.AtlasTexture.Height / EditorInstance.FontSprite.Rect.Height;

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height * heightMul );
				glyphSize *= FontSize * 6;

				var glyphPos = new Rectangle( new Vector2( Bounds.X + x, Bounds.Y + FontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * FontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * FontSize;

				panelRenderer.AddRectangle(
					glyphPos,
					glyphRect,
					ITheme.Current.TextColor
				);
			}

			x += (float)glyph.Advance * FontSize;
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return MeasureText( Text, FontSize );
	}
}
