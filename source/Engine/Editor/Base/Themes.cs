namespace Mocha.Engine.Editor;

public interface ITheme
{
	public static ITheme Current { get; set; } = new TestTheme();

	Vector4 BackgroundColor { get; }
	Vector4 TextColor { get; }
	Vector4 ButtonBgA { get; }
	Vector4 ButtonBgB { get; }
	Vector4 Border { get; }
	float ShadowOpacity { get; }
}

public class DarkTheme : ITheme
{
	public Vector4 BackgroundColor { get; } = MathX.GetColor( "#262626" );

	public Vector4 TextColor { get; } = MathX.GetColor( "#ffffff" );

	public Vector4 ButtonBgA { get; } = MathX.GetColor( "#6e6e6e" );

	public Vector4 ButtonBgB { get; } = MathX.GetColor( "#343434" );

	public Vector4 Border { get; } = MathX.GetColor( "#88171919" );

	public float ShadowOpacity { get; } = 0.125f;
}

public class LightTheme : ITheme
{
	public Vector4 BackgroundColor { get; } = MathX.GetColor( "#e7e7e7" );

	public Vector4 TextColor { get; } = MathX.GetColor( "#000000" );

	public Vector4 ButtonBgA { get; } = MathX.GetColor( "#fefefe" );

	public Vector4 ButtonBgB { get; } = MathX.GetColor( "#f2f2f2" );

	public Vector4 Border { get; } = MathX.GetColor( "#d9d9d9" );

	public float ShadowOpacity { get; } = 0.025f;
}

public class TestTheme : ITheme
{
	public Vector4 BackgroundColor { get; } = MathX.GetColor( "#333438" );

	public Vector4 TextColor { get; } = MathX.GetColor( "#aaffffff" );

	public Vector4 ButtonBgA { get; } = MathX.GetColor( "#53595b" );

	public Vector4 ButtonBgB { get; } = MathX.GetColor( "#3b3e42" );

	public Vector4 Border { get; } = MathX.GetColor( "#551a1c20" );

	public float ShadowOpacity { get; } = 0.1f;
}
