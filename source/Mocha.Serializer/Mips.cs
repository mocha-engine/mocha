namespace Mocha;

public static class Mips
{
	public static int CalcSize( int baseWidth, int mipNumber ) => (int)MathF.Max( 1, MathF.Floor( baseWidth / MathF.Pow( 2, mipNumber ) ) );
}
