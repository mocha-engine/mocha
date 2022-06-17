﻿namespace Mocha.Common;

public static class Screen
{
	public static Point2 Size { get; set; } = new( 1, 1 );

	public static float Aspect => (float)Size.X / (float)Size.Y;

	public static void UpdateFrom( Point2 size )
	{
		Size = size;
	}
}
