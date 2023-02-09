namespace Mocha.Common;

public static class Time
{
	public static float Delta { get; internal set; }
	public static float Now { get; internal set; }
	public static int FPS { get; internal set; }

	public static List<int> FPSHistory { get; } = new();

	private const int TimeScale = 5;

	public static void UpdateFrom( float deltaTime )
	{
		Delta = deltaTime;
		Now = Engine.GetTime();

		FPS = Engine.GetFramesPerSecond().CeilToInt();
		FPSHistory.Add( FPS );

		if ( FPSHistory.Count > TimeScale / Delta )
		{
			FPSHistory.RemoveAt( 0 );
		}
	}
}
