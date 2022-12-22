﻿namespace Mocha.UI;

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

		var texture = new Texture( path );
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

	public static void DrawRect( Rectangle bounds, Vector4 colorTop, Vector4 colorBottom, RoundingFlags roundingFlags = RoundingFlags.None, float roundingRadius = 0f )
	{
		var flags = GetRoundedGraphicsFlags( roundingFlags );
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), colorTop, colorBottom, colorTop, colorBottom, flags, roundingRadius );
	}

	public static void DrawRect( Rectangle bounds, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD, RoundingFlags roundingFlags = RoundingFlags.None, float roundingRadius = 0f )
	{
		var flags = GetRoundedGraphicsFlags( roundingFlags );
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), colorA, colorB, colorC, colorD, flags, roundingRadius );
	}

	public static void DrawRect( Rectangle bounds, Vector4 color, RoundingFlags roundingFlags = RoundingFlags.None, float roundingRadius = 0f )
	{
		var flags = GetRoundedGraphicsFlags( roundingFlags );
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), color, color, color, color, flags, roundingRadius );
	}

	public static void DrawShadow( Rectangle bounds, float size, float opacity, float roundingRadius = 0f )
	{
		bounds = bounds.Expand( size / 2.0f );
		bounds.Position += new Vector2( 0, size / 2.0f );

		for ( float i = 0; i < size; ++i )
		{
			var currBounds = bounds.Shrink( i );
			var color = new Vector4( 0, 0, 0, (1f / size) * opacity );
			PanelRenderer.AddRectangle( currBounds, new Rectangle( 0, 0, 0, 0 ), color, color, color, color, GetRoundedGraphicsFlags( RoundingFlags.All ), roundingRadius );
		}
	}

	public static void DrawRectUnfilled( Rectangle bounds, Vector4 color, float thickness = 1.0f )
	{
		var top = new Rectangle( bounds.X + thickness, bounds.Y, bounds.Width - thickness * 2, thickness );
		DrawRect( top, color );

		var bottom = new Rectangle( bounds.X + thickness, bounds.Y + bounds.Height - thickness, bounds.Width - thickness * 2, thickness );
		DrawRect( bottom, color );

		var left = new Rectangle( bounds.X, bounds.Y, thickness, bounds.Height );
		DrawRect( left, color );

		var right = new Rectangle( bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height );
		DrawRect( right, color );
	}

	internal static void DrawAtlas( Vector2 position )
	{
		const float size = 256;
		float aspect = PanelRenderer.AtlasBuilder.Texture.Width / (float)PanelRenderer.AtlasBuilder.Texture.Height;
		var bounds = new Rectangle( position, new( size * aspect, size ) );
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 1, 1 ), Vector4.One, Vector4.One, Vector4.One, Vector4.One, GraphicsFlags.UseRawImage, 0f );
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
