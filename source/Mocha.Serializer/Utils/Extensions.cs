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
