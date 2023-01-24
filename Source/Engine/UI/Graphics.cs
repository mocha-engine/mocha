namespace Mocha;

[Flags]
public enum RoundingFlags
{
	None = 0,
	TopLeft = 1,
	TopRight = 2,
	BottomLeft = 4,
	BottomRight = 8,

	All = TopLeft | TopRight | BottomLeft | BottomRight,
	Bottom = BottomLeft | BottomRight,
	Top = TopLeft | TopRight,
	Left = TopLeft | BottomLeft,
	Right = TopRight | BottomRight
}

public static partial class Graphics
{
	internal static UIEntity PanelRenderer { get; set; }
	private static Dictionary<string, Texture> CachedTextures { get; } = new();

	private static Texture GetTexture( string path )
	{
		if ( CachedTextures.TryGetValue( path, out var cachedTexture ) )
		{
			return cachedTexture;
		}

		var texture = new Texture( path, false );
		CachedTextures.Add( path, texture );

		return texture;
	}

	private static GraphicsFlags GetRoundedGraphicsFlags( RoundingFlags roundingFlags )
	{
		GraphicsFlags graphicsFlags = GraphicsFlags.None;

		if ( roundingFlags.HasFlag( RoundingFlags.TopLeft ) )
		{
			graphicsFlags |= GraphicsFlags.RoundedTopLeft;
		}

		if ( roundingFlags.HasFlag( RoundingFlags.TopRight ) )
		{
			graphicsFlags |= GraphicsFlags.RoundedTopRight;
		}

		if ( roundingFlags.HasFlag( RoundingFlags.BottomLeft ) )
		{
			graphicsFlags |= GraphicsFlags.RoundedBottomLeft;
		}

		if ( roundingFlags.HasFlag( RoundingFlags.BottomRight ) )
		{
			graphicsFlags |= GraphicsFlags.RoundedBottomRight;
		}

		return graphicsFlags;
	}

	public static void DrawRect( Rectangle bounds, Vector4 color, RoundingFlags roundingFlags = RoundingFlags.None, float roundingRadius = 0f )
	{
		var flags = GetRoundedGraphicsFlags( roundingFlags );
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), color, color, color, color, flags, roundingRadius );
	}

	internal static void DrawTexture( Rectangle bounds, string path )
	{
		var texture = GetTexture( path );
		DrawTexture( bounds, texture );
	}

	internal static void DrawTexture( Rectangle bounds, Texture texture, GraphicsFlags flags = GraphicsFlags.UseRawImage )
	{
		DrawTexture( bounds, texture, Vector4.One, flags );
	}

	internal static void DrawTexture( Rectangle bounds, Texture texture, Vector4 tint, GraphicsFlags flags = GraphicsFlags.UseRawImage )
	{
		var texturePos = PanelRenderer.AtlasBuilder.AddOrGetTexture( texture );
		var textureSize = PanelRenderer.AtlasBuilder.Texture.Size;

		var texBounds = new Rectangle( texturePos, texture.Size );

		// Convert to [0..1] normalized space
		texBounds /= textureSize;

		// Flip y axis
		texBounds.Y = 1.0f - texBounds.Y;

		PanelRenderer.AddRectangle( bounds, texBounds, tint, tint, tint, tint, flags, 0f );
	}
}
