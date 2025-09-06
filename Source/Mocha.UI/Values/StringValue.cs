namespace Mocha.UI;

public class StringValue : Value
{
	public string Value { get; set; }

	public StringValue( string value )
	{
		Value = value;
	}

	public static StringValue ParseFrom( string str )
	{
		var valueStr = str;

		if ( str.EndsWith( "\"" ) && str.StartsWith( "\"" ) )
			valueStr = str[1..^1];

		return new StringValue( valueStr );
	}

	public override string ToString()
	{
		return $"{Value}";
	}
}
