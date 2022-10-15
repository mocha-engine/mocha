using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

public static partial class Graphics
{
	internal static PanelRenderer PanelRenderer { get; set; }

	public static void DrawRect( Rectangle bounds, Vector4 colorTop, Vector4 colorBottom, bool rounded = false )
	{
		var flags = GraphicsFlags.None;
		if ( rounded )
			flags |= GraphicsFlags.Rounded;

		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, colorTop, colorBottom, colorTop, colorBottom, flags );
	}

	public static void DrawRect( Rectangle bounds, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD, bool rounded = false )
	{
		var flags = GraphicsFlags.None;
		if ( rounded )
			flags |= GraphicsFlags.Rounded;

		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, colorA, colorB, colorC, colorD, flags );
	}

	public static void DrawRect( Rectangle bounds, Vector4 color, bool rounded = false )
	{
		var flags = GraphicsFlags.None;
		if ( rounded )
			flags |= GraphicsFlags.Rounded;

		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, color, color, color, color, flags );
	}

	public static void DrawShadow( Rectangle bounds, float size, float opacity )
	{
		bounds = bounds.Expand( size / 2.0f );
		bounds.Position += new Vector2( 0, size / 2.0f );

		for ( float i = 0; i < size; ++i )
		{
			var currBounds = bounds.Shrink( i );
			var color = new Vector4( 0, 0, 0, (1f / size) * opacity );
			PanelRenderer.AddRectangle( currBounds, new Rectangle( 0, 0, 0, 0 ), 0, color, color, color, color, GraphicsFlags.Rounded );
		}
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

	public static void DrawRectUnfilled( Rectangle bounds, Vector4 color, float thickness = 1.0f )
	{
		//
		// Draw lines
		//
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
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 1, 1 ), 0f, Vector4.One, Vector4.One, Vector4.One, Vector4.One, GraphicsFlags.UseRawImage );
	}

	internal static void DrawTexture( Rectangle bounds, Texture texture )
	{
		var texturePos = PanelRenderer.AtlasBuilder.AddOrGetTexture( texture );
		var textureSize = PanelRenderer.AtlasBuilder.Texture.Size;

		var texBounds = new Rectangle( (Vector2)texturePos, texture.Size );

		// Convert to [0..1] normalized space
		texBounds /= textureSize;

		PanelRenderer.AddRectangle( bounds, texBounds, 0, Vector4.One, Vector4.One, Vector4.One, Vector4.One, GraphicsFlags.UseRawImage );
	}
}
