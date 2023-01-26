namespace Mocha.Common;

public struct TimeUntil : IEquatable<TimeUntil>
{
	private float _time;

	public float Absolute => _time;
	public float Until => _time - Time.Now;

	public static implicit operator float( TimeUntil other )
	{
		return other._time - Time.Now;
	}

	public static implicit operator TimeUntil( float other )
	{
		TimeUntil result = new();
		result._time = Time.Now + other;

		return result;
	}

	public override bool Equals( object? obj )
	{
		if ( obj is TimeUntil o )
			return Equals( o );

		return false;
	}

	public bool Equals( TimeUntil o ) => _time == o._time;

	public static bool operator ==( TimeUntil a, TimeUntil b ) => a.Equals( b );
	public static bool operator !=( TimeUntil a, TimeUntil b ) => !a.Equals( b );

	public override int GetHashCode() => base.GetHashCode();
	public override string ToString() => Until.ToString();
}
