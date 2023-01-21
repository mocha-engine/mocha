namespace Mocha.Editor;

public static class Theme
{
	public static Vector4 Green = MathX.GetColor( "#98C379" );
	public static Vector4 Blue = MathX.GetColor( "#62AEEF" );
	public static Vector4 Red = MathX.GetColor( "#E06B74" );
	public static Vector4 Orange = MathX.GetColor( "#E5C07B" );
	public static Vector4 Yellow = MathX.GetColor( "#FFC710" );
	public static Vector4 Gray = MathX.GetColor( "#272929" );
	public static Vector4 Transparent = MathX.GetColor( "#00000000" );

	public static Vector4 ToBackground( this Vector4 vector )
	{
		vector.W = 0.20f;
		return vector;
	}
}
