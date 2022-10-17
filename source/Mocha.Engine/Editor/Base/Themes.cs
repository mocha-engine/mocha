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
	public Vector4 BackgroundColor => MathX.GetColor( "#21252b" );

	public Vector4 TextColor => MathX.GetColor( "#ffffff" );

	public Vector4 ButtonBgA => MathX.GetColor( "#3e4551" );

	public Vector4 ButtonBgB => MathX.GetColor( "#3e4551" );

	public Vector4 Border => MathX.GetColor( "#33000000" );

	public float ShadowOpacity => 0f;
}

public class LightTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#e7e7e7" );

	public Vector4 TextColor => MathX.GetColor( "#000000" );

	public Vector4 ButtonBgA => MathX.GetColor( "#fefefe" );

	public Vector4 ButtonBgB => MathX.GetColor( "#f2f2f2" );

	public Vector4 Border => MathX.GetColor( "#d9d9d9" );

	public float ShadowOpacity => 0.025f;
}

public class TestTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#333438" );

	public Vector4 TextColor => MathX.GetColor( "#aaffffff" );

	public Vector4 ButtonBgA => MathX.GetColor( "#53595b" );

	public Vector4 ButtonBgB => MathX.GetColor( "#3b3e42" );

	public Vector4 Border => MathX.GetColor( "#551a1c20" );

	public float ShadowOpacity => 0.1f;
}
