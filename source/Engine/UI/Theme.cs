namespace Mocha.UI;

static class Theme
{
	public const string Font = "Inter-Regular";
	public const float FontSize = 14;
	public const float ShadowOpacity = 0.25f;

	public static Vector4 TransparentGray { get; } = MathX.GetColor( "#77ABB2BF" );

	public static Vector4 Red { get; } = MathX.GetColor( "#E06B74" );
	public static Vector4 Green { get; } = MathX.GetColor( "#98C379" );
	public static Vector4 Blue { get; } = MathX.GetColor( "#62AEEF" );
	public static Vector4 Orange { get; } = MathX.GetColor( "#E5C07B" );
	public static Vector4 Yellow { get; } = MathX.GetColor( "#FFC710" );

	public static Vector4 Accent { get; set; } = MathX.GetColor( "#2186ff" );

	public static Vector4 BackgroundColor { get; } = MathX.GetColor( "#333438" );
	public static Vector4 TextColor { get; } = MathX.GetColor( "#aaffffff" );
	public static Vector4 ButtonBgA { get; } = MathX.GetColor( "#53595b" );
	public static Vector4 ButtonBgB { get; } = MathX.GetColor( "#3b3e42" );
	public static Vector4 Border { get; } = MathX.GetColor( "#551a1c20" );
}
