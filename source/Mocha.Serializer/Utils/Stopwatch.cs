namespace Mocha.Common;

public class Stopwatch : IDisposable
{
	private readonly DateTime start;
	private readonly string name;

	public Stopwatch( string name )
	{
		Log.Info( $"Starting stopwatch for {name}..." );
		start = DateTime.Now;
		this.name = name;
	}

	void IDisposable.Dispose()
	{
		var end = DateTime.Now;
		var durationMs = (end - start).TotalMilliseconds;

		Log.Info( $"{name} took {durationMs:F0}ms" );
	}
}
