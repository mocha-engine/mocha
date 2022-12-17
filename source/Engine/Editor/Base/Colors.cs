namespace Mocha.Engine.Editor;

static class Colors
{
	public static Vector4 TransparentGray { get; } = MathX.GetColor( "#77ABB2BF" );

	public static Vector4 Red { get; } = MathX.GetColor( "#E06B74" );
	public static Vector4 Green { get; } = MathX.GetColor( "#98C379" );
	public static Vector4 Blue { get; } = MathX.GetColor( "#62AEEF" );
	public static Vector4 Orange { get; } = MathX.GetColor( "#E5C07B" );
	public static Vector4 Yellow { get; } = MathX.GetColor( "#FFC710" );

	public static Vector4 Accent { get; set; } = MathX.GetColor( "#2186ff" );
}
