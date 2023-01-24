using System.Collections.Concurrent;

namespace Mocha;

partial class Graphics
{
	struct CachedFont
	{
		public Texture Texture { get; set; }
		public Font.Data Data { get; set; }
	}

	private static ConcurrentDictionary<string, CachedFont> CachedFonts { get; } = new();

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
		loadedFont.Texture = new Texture( $"core/fonts/{fontName}.mtex" );

		var fileBytes = FileSystem.Game.ReadAllBytes( $"core/fonts/{fontName}.mfnt" );
		loadedFont.Data = Serializer.Deserialize<MochaFile<Font.Data>>( fileBytes ).Data;

		return CachedFonts[fontName] = loadedFont;
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

		if ( CachedTextures.TryGetValue( key, out var cachedTexture ) )
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

				x += 4;
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

				var glyphPos = new Vector2( x, fontData.Atlas.Size );
				glyphPos.X += (float)glyph.PlaneBounds.Left * fontData.Atlas.Size;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * fontData.Atlas.Size;

				glyphPos.X = glyphPos.X.Clamp( 0, float.MaxValue );
				glyphPos.Y = glyphPos.Y.Clamp( 0, float.MaxValue );

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

		size.X = size.X.Clamp( 1, 1000f );
		size.Y = size.Y.Clamp( 1, 1000f );

		return size * scale;
	}

	public static void DrawText( Rectangle bounds, string text, string fontFamily, float fontSize, Vector4 color )
	{
		float x = 0;

		var font = LoadOrGetFont( fontFamily );

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
				x += fontSize;
				continue;
			}

			var glyph = fontData.Glyphs.First( x => x.Unicode == (int)c );

			if ( glyph.AtlasBounds != null )
			{
				var glyphRect = FontBoundsToAtlasRect( glyph );

				var glyphSize = new Vector2( glyphRect.Width, glyphRect.Height );
				glyphSize *= fontSize / fontData.Atlas.Size;

				var glyphPos = new Rectangle( new Vector2( bounds.X + x, bounds.Y + fontSize ), glyphSize );
				glyphPos.X += (float)glyph.PlaneBounds.Left * fontSize;
				glyphPos.Y -= (float)glyph.PlaneBounds.Top * fontSize;

				float screenPxRange = fontData.Atlas.DistanceRange * (glyphPos.Size / fontData.Atlas.Size).Length;
				screenPxRange *= 1.5f;

				if ( glyphPos.X > bounds.X + bounds.Width && bounds.Width > 0 )
					return;

				Graphics.DrawCharacter(
					glyphPos,
					fontTexture,
					glyphRect,
					color
				);
			}

			x += (float)glyph.Advance * fontSize;

			// HACK HACK cont.
			if ( c > FontAwesome.IconMin )
			{
				x += 4;
			}
		}
	}

	public static void DrawCharacter( Rectangle bounds, Texture texture, Rectangle atlasBounds, Vector4 color )
	{
		var flags = GraphicsFlags.UseSdf;
		if ( bounds.Size.Length > 20f )
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

		PanelRenderer.AddRectangle( bounds, atlasBounds, color, color, color, color, flags, 0f );
	}
}
