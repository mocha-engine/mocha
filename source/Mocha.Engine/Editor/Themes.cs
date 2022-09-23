namespace Mocha.Engine.Editor;

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
	public Vector4 BackgroundColor => MathX.GetColor( "#1c1917" );

	public Vector4 TextColor => MathX.GetColor( "#ffffff" );

	public Vector4 ButtonBgA => MathX.GetColor( "#44403c" );

	public Vector4 ButtonBgB => MathX.GetColor( "#292524" );

	public Vector4 Border => MathX.GetColor( "#050506" );
}

public class LightTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#e7e7e7" );

	public Vector4 TextColor => MathX.GetColor( "#000000" );

	public Vector4 ButtonBgA => MathX.GetColor( "#fefefe" );

	public Vector4 ButtonBgB => MathX.GetColor( "#f2f2f2" );

	public Vector4 Border => MathX.GetColor( "#d9d9d9" );
}

public class TestTheme : ITheme
{
	public Vector4 BackgroundColor => MathX.GetColor( "#141416" );

	public Vector4 TextColor => MathX.GetColor( "#ffffff" );

	public Vector4 ButtonBgA => MathX.GetColor( "#29292b" );

	public Vector4 ButtonBgB => MathX.GetColor( "#1f1f22" );

	public Vector4 Border => MathX.GetColor( "#343436" );
}
