namespace Mocha.Common;

public static class Screen
{
	public static Point2 Size { get; set; } = new( 1280, 720 );

	public static float Aspect => Size.X / (float)Size.Y;

	public static void UpdateFrom( Vector2 size )
	{
		Size = new Point2( (int)size.X, (int)size.Y );
	}
}
