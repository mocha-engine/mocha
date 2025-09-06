namespace Mocha.UI;

public interface IRenderer
{
	void DrawRectangle( Rectangle bounds, ColorValue color, float rounding = 0 );
	void DrawText( Rectangle bounds, string text, string font, int weight, float fontSize, ColorValue color );
	void DrawImage( Rectangle bounds, string path );
	Vector2 CalcTextSize( string text, string fontFamily, int weight, float fontSize );
}
