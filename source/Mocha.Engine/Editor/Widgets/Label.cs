namespace Mocha.Engine.Editor;

internal class Label : Widget
{
	private string calculatedText = "";
	private string text;

	public string Text
	{
		get => text; set
		{
			text = value;
			CalculateText();
		}
	}
	public Vector4 Color { get; set; } = ITheme.Current.TextColor;
	public float FontSize { get; set; } = 14f;

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

	internal Label( string text, float fontSize = 14f ) : base()
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

	internal override void Render()
	{
		float x = 0;

		foreach ( var c in calculatedText )
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

				float screenPxRange = EditorInstance.FontData.Atlas.DistanceRange * (glyphPos.Size / EditorInstance.FontData.Atlas.Size).Length;
				screenPxRange *= 1.5f;

				Graphics.DrawRect(
					glyphPos,
					glyphRect,
					screenPxRange,
					Color
				);
			}

			x += (float)glyph.Advance * FontSize;
		}
	}

	internal override Vector2 GetDesiredSize()
	{
		return MeasureText( Text, FontSize );
	}

	internal override void OnBoundsChanged()
	{
		CalculateText();
	}

	private void CalculateText()
	{
		var text = Text;

		if ( MeasureText( text, FontSize ).X > Bounds.Width )
		{
			for ( int i = 0; i < text.Length; ++i )
			{
				var search = text[..^i] + "...";
				var searchSize = MeasureText( search, FontSize );

				if ( char.IsWhiteSpace( search[^1] ) )
					continue;

				if ( searchSize.X < Bounds.Width )
				{
					text = search;
					break;
				}
			}
		}

		calculatedText = text;
	}
}
