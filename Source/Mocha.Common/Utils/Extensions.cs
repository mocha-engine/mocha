namespace Mocha.Common;

public static class ByteExtension
{
	public static bool GetBit( this byte self, int index )
	{
		var value = ((self >> (7 - index)) & 1) != 0;
		return value;
	}
	public static bool[] GetBits( this byte self, params int[] indices )
	{
		var bits = new bool[indices.Length];
		for ( var i = 0; i < indices.Length; ++i )
		{
			bits[i] = self.GetBit( indices[i] );
		}

		return bits;
	}
}

public static class BoolArrayExtension
{
	public static bool ValuesEqual( this bool[] self, bool[] other )
	{
		for ( var i = 0; i < Math.Min( self.Length, other.Length ); ++i )
		{
			if ( self[i] != other[i] )
				return false;
		}

		return true;
	}
}

public static class ListExtension
{
	public static T Pop<T>( this IList<T> list )
	{
		T item = list.First();
		list.RemoveAt( 0 );
		return item;
	}

	public static void Push<T>( this IList<T> list, T item )
	{
		list.Add( item );
	}
}

public static class StringExtension
{
	public static string NormalizePath( this string str )
	{
		return str.Replace( "\\", "/" );
	}

	public static string Pad( this string str )
	{
		return str.PadRight( 16 );
	}

	public static string DisplayName( this string str )
	{
		string result = "";

		for ( int i = 0; i < str.Length; ++i )
		{
			char c = str[i];
			if ( i != 0 && char.IsUpper( c ) )
				result += " ";

			result += c;
		}

		return result;
	}

	public static bool TryConvert( this string str, Type t, out object? Value )
	{
		Value = null;

		if ( t == typeof( string ) )
		{
			Value = str;
			return true;
		}

		if ( t == typeof( float ) )
		{
			Value = str.ToFloat();
			return true;
		}

		if ( t == typeof( double ) )
		{
			Value = str.ToDouble();
			return true;
		}

		if ( t == typeof( int ) )
		{
			Value = str.ToInt();
			return true;
		}

		if ( t == typeof( uint ) )
		{
			Value = str.ToUInt();
			return true;
		}

		if ( t == typeof( long ) )
		{
			Value = str.ToLong();
			return true;
		}

		if ( t == typeof( ulong ) )
		{
			Value = str.ToULong();
			return true;
		}

		if ( t == typeof( bool ) )
		{
			Value = str.ToBool();
			return true;
		}

		return false;
	}

	public static float ToFloat( this string str, float Default = default )
	{
		return float.TryParse( str, out var result ) ? result : Default;
	}

	public static double ToDouble( this string str, double Default = default )
	{
		return double.TryParse( str, out var result ) ? result : Default;
	}

	public static int ToInt( this string str, int Default = default )
	{
		return int.TryParse( str, out var result ) ? result : Default;
	}

	public static uint ToUInt( this string str, uint Default = default )
	{
		return uint.TryParse( str, out var result ) ? result : Default;
	}

	public static long ToLong( this string str, long Default = default )
	{
		return long.TryParse( str, out var result ) ? result : Default;
	}

	public static ulong ToULong( this string str, ulong Default = default )
	{
		return ulong.TryParse( str, out var result ) ? result : Default;
	}

	public static bool ToBool( this string str )
	{
		switch ( str )
		{
			case null:
			case "":
			case "0":
				return false;

			default:
				if ( str.Equals( "yes", StringComparison.OrdinalIgnoreCase ) ||
					str.Equals( "true", StringComparison.OrdinalIgnoreCase ) ||
					str.Equals( "on", StringComparison.OrdinalIgnoreCase ) )
					return true;

				// If it fails to parse as a number, it returns false
				// If it succeeds and it's not 0, it returns true
				return float.TryParse( str, out var result ) && result != 0.0f;
		}
	}
}
