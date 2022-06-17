namespace Mocha.Common;

public class Stopwatch : IDisposable
{
	private DateTime start;
	private string name;

	public Stopwatch( string name )
	{
		Log.Info( $"Starting stopwatch for {name}..." );
		start = DateTime.Now;
		this.name = name;
	}

	void IDisposable.Dispose()
	{
		var end = DateTime.Now;
		var duration = (end - start).TotalMilliseconds;

		if ( duration > 2500 )
			Log.Warning( $"{name} took {duration / 1000:F0}s!" );
		else
			Log.Info( $"{name} took {duration:F0}ms" );
	}
}
