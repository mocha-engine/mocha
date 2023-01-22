namespace Mocha.Common;

public struct TimeUntil : IEquatable<TimeUntil>
{
	private float time;

	public float Absolute => time;
	public float Until => time - Time.Now;

	public static implicit operator float( TimeUntil other )
	{
		return other.time - Time.Now;
	}

	public static implicit operator TimeUntil( float other )
	{
		TimeUntil result = new();
		result.time = Time.Now + other;

		return result;
	}

	public override bool Equals( object? obj )
	{
		if ( obj is TimeUntil o )
			return Equals( o );

		return false;
	}

	public bool Equals( TimeUntil o ) => time == o.time;

	public static bool operator ==( TimeUntil a, TimeUntil b ) => a.Equals( b );
	public static bool operator !=( TimeUntil a, TimeUntil b ) => !a.Equals( b );

	public override int GetHashCode() => base.GetHashCode();
	public override string ToString() => Until.ToString();
}
