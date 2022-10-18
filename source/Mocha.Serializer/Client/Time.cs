namespace Mocha.Common;

public class Time
{
	public static float Delta { get; internal set; }
	public static float Now { get; internal set; }

	public static float[] DeltaHistory { get; } = new float[512];
	public static float AverageDelta { get; set; } = 1.0f;

	private static TimeSince TimeSinceAverageCalculated = 0;

	public static void UpdateFrom( float deltaTime )
	{
		Delta = deltaTime;
		Now += deltaTime;

		for ( int i = DeltaHistory.Length - 1; i > 0; i-- )
		{
			DeltaHistory[i] = DeltaHistory[i - 1];
		}

		DeltaHistory[0] = Delta;

		if ( TimeSinceAverageCalculated > 1 )
		{
			AverageDelta = DeltaHistory.Average();
			TimeSinceAverageCalculated = 0;
		}
	}
}
