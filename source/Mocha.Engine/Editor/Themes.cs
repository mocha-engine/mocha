namespace Mocha.Engine;

public interface ITheme
{
	public static ITheme Current { get; set; } = new DarkTheme();

	Vector4 BackgroundColor { get; }
	Vector4 TextColor { get; }
	Vector4 ButtonBgA { get; }
	Vector4 ButtonBgB { get; }
	Vector4 Border { get; }
}

public class DarkTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#333333" );

	public Vector4 TextColor => MathX.GetColor( "#ffffff" );

	public Vector4 ButtonBgA => MathX.GetColor( "#5c5c5c" );

	public Vector4 ButtonBgB => MathX.GetColor( "#3d3d3d" );

	public Vector4 Border => MathX.GetColor( "#2f2f2f" );
}

public class LightTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#e7e7e7" );

	public Vector4 TextColor => MathX.GetColor( "#000000" );

	public Vector4 ButtonBgA => MathX.GetColor( "#fefefe" );

	public Vector4 ButtonBgB => MathX.GetColor( "#f2f2f2" );

	public Vector4 Border => MathX.GetColor( "#d9d9d9" );
}
