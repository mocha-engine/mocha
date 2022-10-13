namespace Mocha.Engine.Editor;

static class Colors
{
	public static Vector4 TransparentGray => MathX.GetColor( "#77ABB2BF" );

	public static Vector4 Red => MathX.GetColor( "#E06B74" );
	public static Vector4 Green => MathX.GetColor( "#98C379" );
	public static Vector4 Blue => MathX.GetColor( "#62AEEF" );
	public static Vector4 Orange => MathX.GetColor( "#E5C07B" );
	public static Vector4 Yellow => MathX.GetColor( "#FFC710" );

	public static Vector4 Accent { get; set; } = MathX.GetColor( "#2186ff" );
}
