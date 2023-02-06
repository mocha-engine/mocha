namespace Mocha.Common;

public static class Time
{
	public static float Delta { get; internal set; }
	public static float Now { get; internal set; }
	public static int FPS { get; internal set; }

	public static List<int> FPSHistory { get; } = new();

	private static TimeSince s_timeSinceLastClear;

	private const int TimeScale = 5;

	public static void UpdateFrom( float deltaTime )
	{
		Delta = deltaTime;
		Now = Glue.Engine.GetTime();

		FPS = Glue.Engine.GetFramesPerSecond().CeilToInt();
		FPSHistory.Add( FPS );

		if ( s_timeSinceLastClear > TimeScale )
		{
			FPSHistory.RemoveAt( 0 );
			s_timeSinceLastClear = 0;
		}
	}
}
