namespace Mocha.UI;

public class LengthValue : Value
{
	public float Value { get; set; }

	public LengthValue( float value )
	{
		Value = value;
	}
	public static LengthValue Zero => new LengthValue( 0 );

	public static LengthValue ParseFrom( string str )
	{
		var valueStr = str;

		if ( str.EndsWith( "px" ) )
			valueStr = str[..^2];

		if ( float.TryParse( valueStr, out var value ) )
			return new LengthValue( value );

		return Zero;
	}

	public override string ToString()
	{
		return $"{Value}px";
	}
}
