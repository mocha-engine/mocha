using System.Globalization;
using System.Numerics;

namespace Mocha.UI;

public class ColorValue : Value
{
	public ColorValue() { }

	public ColorValue( byte red, byte green, byte blue, byte alpha )
	{
		Red = red;
		Green = green;
		Blue = blue;
		Alpha = alpha;
	}

	public static ColorValue White => new ColorValue( 255, 255, 255, 255 );
	public static ColorValue Transparent => new ColorValue( 255, 255, 255, 0 );

	public byte Red { get; set; }
	public byte Green { get; set; }
	public byte Blue { get; set; }
	public byte Alpha { get; set; }

	public static ColorValue ParseFrom( string str )
	{
		if ( !str.StartsWith( "#" ) )
			return White;

		var valueStr = str[1..];

		string rStr = "";
		string gStr = "";
		string bStr = "";
		string aStr = "";

		switch ( valueStr.Length )
		{
			case 3:
				// #RGB
				rStr = valueStr[0..1];
				gStr = valueStr[1..2];
				bStr = valueStr[2..3];
				aStr = "ff";
				break;
			case 4:
				// #RGBA
				rStr = valueStr[0..1];
				gStr = valueStr[1..2];
				bStr = valueStr[2..3];
				aStr = valueStr[3..4];
				break;
			case 6:
				// #RRGGBB
				rStr = valueStr[0..2];
				gStr = valueStr[2..4];
				bStr = valueStr[4..6];
				aStr = "ff";
				break;
			case 8:
				// #RRGGBBAA
				rStr = valueStr[0..2];
				gStr = valueStr[2..4];
				bStr = valueStr[4..6];
				aStr = valueStr[6..8];
				break;
			default:
				return White;
				break;
		}

		var colorValue = new ColorValue();

		if ( byte.TryParse( rStr, NumberStyles.HexNumber, null, out var red ) )
			colorValue.Red = red;

		if ( byte.TryParse( gStr, NumberStyles.HexNumber, null, out var green ) )
			colorValue.Green = green;

		if ( byte.TryParse( bStr, NumberStyles.HexNumber, null, out var blue ) )
			colorValue.Blue = blue;

		if ( byte.TryParse( aStr, NumberStyles.HexNumber, null, out var alpha ) )
			colorValue.Alpha = alpha;

		return colorValue;
	}

	public override string ToString()
	{
		return $"{Red}, {Green}, {Blue}, {Alpha}";
	}

	public Vector4 ToVector4()
	{
		return new Vector4(
			Red / 255f,
			Green / 255f,
			Blue / 255f,
			Alpha / 255f
		);
	}
}
