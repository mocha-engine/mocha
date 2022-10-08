using Mocha.Renderer.UI;

namespace Mocha.Engine.Editor;

public static class Graphics
{
	internal static PanelRenderer PanelRenderer { get; set; }

	public static void DrawRect( Rectangle bounds, Rectangle uvBounds, float screenPxRange, Vector4 color )
	{
		PanelRenderer.AddRectangle( bounds, uvBounds, screenPxRange, color, color, color, color );
	}

	public static void DrawRect( Rectangle bounds, Vector4 colorTop, Vector4 colorBottom )
	{
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, colorTop, colorBottom, colorTop, colorBottom );
	}

	public static void DrawRect( Rectangle bounds, Vector4 colorA, Vector4 colorB, Vector4 colorC, Vector4 colorD )
	{
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, colorA, colorB, colorC, colorD );
	}

	public static void DrawRect( Rectangle bounds, Vector4 color )
	{
		PanelRenderer.AddRectangle( bounds, new Rectangle( 0, 0, 0, 0 ), 0, color, color, color, color );
	}

	public static void DrawShadow( Rectangle bounds, float size, float opacity )
	{
		bounds = bounds.Expand( size / 2.0f );
		bounds.Position += new Vector2( 0, size / 2.0f );

		for ( float i = 0; i < size; ++i )
		{
			var currBounds = bounds.Shrink( i );
			var color = new Vector4( 0, 0, 0, (1f / size) * opacity );
			PanelRenderer.AddRectangle( currBounds, new Rectangle( 0, 0, 0, 0 ), 0, color, color, color, color );
		}
	}

	public static void DrawText()
	{
		throw new NotImplementedException();
	}

	public static void DrawRoundedRectangle()
	{
		throw new NotImplementedException();
	}

	public static void DrawRectUnfilled( Rectangle bounds, Vector4 color )
	{
		//
		// Draw lines
		//

		var lineA = new Rectangle( bounds.X, bounds.Y, bounds.Width, 1 );
		var lineB = new Rectangle( bounds.X, bounds.Y, 1, bounds.Height );
		var lineC = new Rectangle( bounds.X + bounds.Width - 1, bounds.Y, 1, bounds.Height );
		var lineD = new Rectangle( bounds.X, bounds.Y + bounds.Height - 1, bounds.Width, 1 );

		DrawRect( lineA, color );
		DrawRect( lineB, color );
		DrawRect( lineC, color );
		DrawRect( lineD, color );
	}
}
