namespace Mocha.Engine;

static class OneDark
{
	public static System.Numerics.Vector4 Background => MathX.GetColor( "#282C34" );
	public static System.Numerics.Vector4 Variable => MathX.GetColor( "#E06C75" );
	public static System.Numerics.Vector4 String => MathX.GetColor( "#98C379" );
	public static System.Numerics.Vector4 Literal => MathX.GetColor( "#E5C07B" );
	public static System.Numerics.Vector4 Label => MathX.GetColor( "#61AFEF" );
	public static System.Numerics.Vector4 Instruction => MathX.GetColor( "#C678DD" );
	public static System.Numerics.Vector4 Comment => MathX.GetColor( "#56B6C2" );
	public static System.Numerics.Vector4 Generic => MathX.GetColor( "#ABB2BF" );
	public static System.Numerics.Vector4 DullGeneric => MathX.GetColor( "#4F5259" );
	public static System.Numerics.Vector4 Step => MathX.GetColor( "#C8CC76" );

	public static System.Numerics.Vector4 Error => MathX.GetColor( "#E06B74" );
	public static System.Numerics.Vector4 Trace => MathX.GetColor( "#ABB2BF" );
	public static System.Numerics.Vector4 Warning => MathX.GetColor( "#E5C07B" );
	public static System.Numerics.Vector4 Info => MathX.GetColor( "#62AEEF" );
}
