using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

partial class Graphics
{
	struct CachedFont
	{
		public Texture Texture { get; set; }
		public Font.Data Data { get; set; }
	}

	private static Dictionary<string, CachedFont> CachedFonts { get; } = new();

	private static Dictionary<string, Texture> CachedStringTextures { get; } = new();

	private static Texture GetTextureForText( string text, string fontFamily, float fontSize )
	{
		var key = GetKeyForText( text, fontFamily );

		if ( CachedStringTextures.TryGetValue( key, out var cachedTexture ) )
		{
			return cachedTexture;
		}

		var font = LoadOrGetFont( fontFamily );

		var targetSize = MeasureText( text, fontFamily, font.Data.Atlas.Size ) + 32;
		var stitcher = new TextureStitcher( (int)targetSize.X, (int)targetSize.Y );
		stitcher.Texture.Path = "internal:editor_" + fontFamily + "_" + text + "_" + fontSize;

		float x = 0;
		foreach ( var c in text )
		{
			var fontData = font.Data;
			var fontTexture = font.Texture;

			//
			// HACK HACK: Hard check to see if this is a font awesome glyph, swap the font
			// data and texture out temporarily
			//
			if ( c > FontAwesome.IconMin )
			{
				var fontAwesome = LoadOrGetFont( "fa-solid-900" );
				fontData = fontAwesome.Data;
				fontTexture = fontAwesome.Texture;
			}

			if ( !fontData.Glyphs.Any( x => x.Unicode == c ) )
			{
				x += fontData.Atlas.Size;
				continue;
			}

			var glyph = fontData.Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph );
				glyphRect.Y = fontTexture.Height - glyphRect.Y;

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
				// glyphSize *= fontSize / fontData.Atlas.Size;

				var glyphPos = new Vector2( x, 32 );
				glyphPos.X += (float)glyph.PlaneBounds.Left;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * 32f;

				stitcher.AddTexture( glyphPos, glyphRect.Position, glyphRect.Size, fontTexture );
			}

			x += (float)glyph.Advance * fontData.Atlas.Size;// * fontSize;
		}

		CachedStringTextures.Add( key, stitcher.Texture );
		return stitcher.Texture;
	}

	private static string GetKeyForText( string text, string fontName )
	{
		return text + "_" + fontName;
	}

	private static CachedFont LoadOrGetFont( string fontName )
	{
		if ( CachedFonts.TryGetValue( fontName, out var cachedFont ) )
		{
			return cachedFont;
		}

		var loadedFont = new CachedFont();
		loadedFont.Texture = Texture.Builder.FromPath( $"core/fonts/baked/{fontName}.mtex" ).Build();
		loadedFont.Data = FileSystem.Game.Deserialize<Font.Data>( $"core/fonts/baked/{fontName}.json" );

		return CachedFonts[fontName] = loadedFont;
	}

	public static void DrawCharacter( Rectangle bounds, Texture texture, Rectangle atlasBounds, Vector4 color )
	{
		var flags = GraphicsFlags.UseSdf;
		if ( bounds.Size.Length > 16f )
			flags |= GraphicsFlags.HighDistMul;

		var texturePos = PanelRenderer.AtlasBuilder.AddOrGetTexture( texture );
		var textureSize = PanelRenderer.AtlasBuilder.Texture.Size;

		var texBounds = new Rectangle( (Vector2)texturePos, textureSize );

		// Move to top left of texture inside atlas
		atlasBounds.Y += textureSize.Y - texture.Height;
		atlasBounds.X += texBounds.X;

		// Convert to [0..1] normalized space
		atlasBounds /= textureSize;

		// Flip y axis
		atlasBounds.Y = 1.0f - atlasBounds.Y;

		PanelRenderer.AddRectangle( bounds, atlasBounds, 0, color, color, color, color, flags );
	}

	private static Rectangle FontBoundsToAtlasRect( Font.Glyph glyph )
	{
		Vector2 min = new Vector2( glyph.AtlasBounds.Left,
								  glyph.AtlasBounds.Top );

		Vector2 max = new Vector2( glyph.AtlasBounds.Right,
								  glyph.AtlasBounds.Bottom );

		Vector2 mins = min;
		Vector2 maxs = (max - min) * new Vector2( 1, -1 );

		var glyphRect = new Rectangle( mins, maxs );

		return glyphRect;
	}

	public static Vector2 MeasureText( string text, string fontFamily, float fontSize )
	{
		float x = 0;

		var font = LoadOrGetFont( fontFamily );
		var fontData = font.Data;
		var fontTexture = font.Texture;

		foreach ( var c in text )
		{
			if ( !fontData.Glyphs.Any( x => x.Unicode == c ) )
			{
				x += fontSize;
				continue;
			}

			var glyph = fontData.Glyphs.First( x => x.Unicode == c );
			x += (float)glyph.Advance * fontSize;
		}

		return new Vector2( x, fontSize * 1.25f ); // ???
	}

	public static Vector2 MeasureText( string text, float fontSize = 12 )
	{
		return MeasureText( text, "qaz", fontSize );
	}

	public static void DrawText( Rectangle bounds, string text, float fontSize = 12 )
	{
		DrawText( bounds, text, "qaz", fontSize );
	}

	public static void DrawText( Rectangle bounds, string text, Vector4 color, float fontSize = 12 )
	{
		DrawText( bounds, text, "qaz", fontSize, color );
	}

	public static void DrawText( Rectangle bounds, string text, string fontFamily, float fontSize )
	{
		DrawText( bounds, text, fontFamily, fontSize, ITheme.Current.TextColor );
	}

	public static void DrawText( Rectangle bounds, string text, string fontFamily, float fontSize, Vector4 color )
	{
		var flags = GraphicsFlags.UseSdf;
		if ( bounds.Size.Length > 16f )
			flags |= GraphicsFlags.HighDistMul;

		var texture = GetTextureForText( text, fontFamily, fontSize );
		bounds.Width = texture.Width * (fontSize / 32f);
		bounds.Height = texture.Height * (fontSize / 32f);

		DrawTexture( bounds, texture, color, flags );
	}
}
