namespace Mocha.Engine.Editor;

internal class Label : Widget
{
	private string calculatedText = "";
	private string text;

	// TODO: font loader & cache
	internal Font.Data FontData { get; set; }
	internal Texture FontTexture { get; set; }

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

	public Vector2 MeasureText( string text, float fontSize )
	{
		float x = 0;

		foreach ( var c in text )
		{
			var glyph = FontData.Glyphs.First( x => x.Unicode == c );
			x += (float)glyph.Advance * fontSize;
		}

		return new Vector2( x, fontSize * 1.25f );
	}

	internal Label( string text, float fontSize = 14f, string fontFamily = "Inter" ) : base()
	{
		FontSize = fontSize;
		Text = text;

		SetFont( fontFamily );
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

		// glyphRect.Y -= EditorInstance.FontSprite.Rect.Y;
		// glyphRect.Y += EditorInstance.AtlasTexture.Height - EditorInstance.FontSprite.Rect.Height - EditorInstance.FontSprite.Rect.Y;
		// glyphRect.X += EditorInstance.FontSprite.Rect.X;
		// glyphRect /= EditorInstance.AtlasTexture.Size;
		// glyphRect.Y = 1.0f - glyphRect.Y;

		return glyphRect;
	}

	internal override void Render()
	{
		float x = 0;

		if ( FontData == null )
			return;

		if ( FontTexture == null )
			return;

		foreach ( var c in calculatedText )
		{
			var glyph = FontData.Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph, glyph.AtlasBounds );

				// float heightMul = EditorInstance.AtlasTexture.Height / EditorInstance.FontSprite.Rect.Height;
				// float widthMul = EditorInstance.AtlasTexture.Width / EditorInstance.FontSprite.Rect.Width;

				float widthMul = 1.0f;
				float heightMul = 1.0f;

				var glyphSize = new Vector2( glyphRect.Width * widthMul, glyphRect.Height * heightMul );
				glyphSize *= FontSize * 0.03f;

				var glyphPos = new Rectangle( new Vector2( Bounds.X + x, Bounds.Y + FontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * FontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * FontSize;

				float screenPxRange = FontData.Atlas.DistanceRange * (glyphPos.Size / FontData.Atlas.Size).Length;
				screenPxRange *= 1.5f;

				if ( glyphPos.X > Bounds.X + Bounds.Width && Bounds.Width > 0 )
					return;

				Graphics.DrawCharacter(
					glyphPos,
					FontTexture,
					glyphRect,
					Color
				);
			}

			x += (float)glyph.Advance * FontSize;
		}
	}

	public void SetFont( string fontName )
	{
		FontData = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{fontName}.json" );
		FontTexture = Texture.Builder.FromPath( $"core/fonts/baked/{fontName}.mtex" ).Build();
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
		calculatedText = text;
	}
}
