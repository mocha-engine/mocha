namespace Mocha.UI;

public class EnumValue : Value
{
	public string Value { get; set; }

	public EnumValue( string value )
	{
		Value = value;
	}

	public static EnumValue ParseFrom( string str )
	{
		return new( str );
	}

	public T GetValue<T>() where T : struct, Enum
	{
		if ( Enum.TryParse<T>( Value.Replace( "-", "" ), true, out var value ) )
			return value;

		return default;
	}

	public override string ToString()
	{
		return Value;
	}
}
