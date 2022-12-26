namespace Mocha.Common;

public class Time
{
	public static float Delta { get; internal set; }
	public static float Now { get; internal set; }
	public static int FPS { get; internal set; }

	public static List<int> FPSHistory { get; } = new();

	public static void UpdateFrom( float deltaTime )
	{
		Delta = deltaTime;
		Now += deltaTime;

		FPS = (1.0f / Delta).CeilToInt();

		FPSHistory.Add( FPS );
		if ( FPSHistory.Count > 512 )
			FPSHistory.RemoveAt( 0 );
	}
}
