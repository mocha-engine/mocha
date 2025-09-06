namespace Mocha.Common;

public class Stopwatch : IDisposable
{
	private readonly DateTime _start;
	private readonly string _name;

	public Stopwatch( string name )
	{
		Log.Info( $"Starting stopwatch for {name}..." );
		_start = DateTime.Now;
		this._name = name;
	}

	void IDisposable.Dispose()
	{
		var end = DateTime.Now;
		var durationMs = (end - _start).TotalMilliseconds;

		Log.Info( $"{_name} took {durationMs:F0}ms" );
	}
}
