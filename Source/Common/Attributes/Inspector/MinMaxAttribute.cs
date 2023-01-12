namespace Mocha.Common;

/// <summary>
/// Marks a property to override the default Min/Max value of the property editor.
/// </summary>
[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
public class MinMaxAttribute : Attribute
{
	public int Min { get; }
	public int Max { get; }

	public MinMaxAttribute( int min, int max )
	{
		Min = min;
		Max = max;
	}

	public MinMaxAttribute( int max ) : this( 0, max )
	{
	}
}
