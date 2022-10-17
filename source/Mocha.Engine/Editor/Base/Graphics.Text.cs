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

		var targetSize = MeasureText( text, fontFamily, font.Data.Atlas.Size );
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

			var glyph = fontData.Glyphs.FirstOrDefault( x => x.Unicode == (int)c );

			// If we don't have anything for this glyph, render a blank space
			// (should probably just draw the missing texture though)
			if ( glyph == null )
			{
				x += fontData.Atlas.Size;
				continue;
			}

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph );
				glyphRect.Y = fontTexture.Height - glyphRect.Y;

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );

				var glyphPos = new Vector2( x, 32 );
				glyphPos.X += (float)glyph.PlaneBounds.Left * 4f;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * 32f;

				stitcher.AddTexture( glyphPos, glyphRect.Position, glyphRect.Size, fontTexture );
			}

			x += (float)glyph.Advance * fontData.Atlas.Size;
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

		var texBounds = new Rectangle( texturePos, textureSize );

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
		var size = new Vector2();
		var key = GetKeyForText( text, fontFamily );
		var font = LoadOrGetFont( fontFamily );
		var scale = (fontSize / font.Data.Atlas.Size);

		if ( CachedStringTextures.TryGetValue( key, out var cachedTexture ) )
		{
			return cachedTexture.Size * scale;
		}

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

				x += 32;
			}

			var glyph = fontData.Glyphs.FirstOrDefault( x => x.Unicode == (int)c );

			// If we don't have anything for this glyph, render a blank space
			// (should probably just draw the missing texture though)
			if ( glyph == null )
			{
				x += fontData.Atlas.Size;
				continue;
			}

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph );
				glyphRect.Y = fontTexture.Height - glyphRect.Y;

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );

				var glyphPos = new Vector2( x, 32 );
				glyphPos.X += (float)glyph.PlaneBounds.Left * 4f;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * 32f;

				if ( glyphPos.X + glyphSize.X > size.X )
				{
					size.X = glyphPos.X + glyphSize.X;
				}

				if ( glyphPos.Y + glyphSize.Y > size.Y )
				{
					size.Y = glyphPos.Y + glyphSize.Y;
				}
			}

			x += (float)glyph.Advance * fontData.Atlas.Size;
		}

		//
		// HACK: If texture is blank, allocate one anyway
		//
		if ( size.X < 1 )
			size.X = 1;

		if ( size.Y < 1 )
			size.Y = 1;

		return size * scale;
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
		if ( fontSize > 24f )
			flags |= GraphicsFlags.HighDistMul;

		var texture = GetTextureForText( text, fontFamily, fontSize );
		bounds.Width = texture.Width * (fontSize / 32f);
		bounds.Height = texture.Height * (fontSize / 32f);

		DrawTexture( bounds, texture, color, flags );
	}
}
