namespace Mocha.Editor;

public static class Theme
{
	public static readonly Vector4 Green = MathX.GetColor( "#98C379" );
	public static readonly Vector4 Blue = MathX.GetColor( "#62AEEF" );
	public static readonly Vector4 Red = MathX.GetColor( "#E06B74" );
	public static readonly Vector4 Orange = MathX.GetColor( "#E5C07B" );
	public static readonly Vector4 Yellow = MathX.GetColor( "#FFC710" );
	public static readonly Vector4 Gray = MathX.GetColor( "#272929" );
	public static readonly Vector4 Transparent = MathX.GetColor( "#00000000" );
	public static readonly Vector4 LightGray = MathX.GetColor( "#CCCCCC" );
	public static readonly Vector4 DarkGray = MathX.GetColor( "#111111" );

	public static Vector4 ToBackground( this Vector4 vector, float opacity = 0.2f )
	{
		vector.W = opacity;
		return vector;
	}
}
