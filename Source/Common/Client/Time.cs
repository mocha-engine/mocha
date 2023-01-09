namespace Mocha.Common;

public class Time
{
	public static float Delta { get; internal set; }
	public static float Now { get; internal set; }
	public static int FPS { get; internal set; }

	public static List<int> FPSHistory { get; } = new();

	public static void Update( float deltaTime, float currentTime, int framesPerSecond )
	{
		Delta = deltaTime;
		Now = currentTime;

		FPS = framesPerSecond;

		FPSHistory.Add( FPS );
		if ( FPSHistory.Count > 512 )
			FPSHistory.RemoveAt( 0 );
	}
}
