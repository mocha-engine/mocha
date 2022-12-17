namespace Mocha.Common;

public static class Screen
{
	public static Point2 Size { get; set; } = new( 1920, 1080 ); // TODO

	public static float Aspect => Size.X / (float)Size.Y;

	public static void UpdateFrom( Point2 size )
	{
		Size = size;
	}
}
