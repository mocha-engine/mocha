using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mocha.Glue;
public static class InteropUtils
{
	public static IntPtr GetPtr( object obj )
	{
		if ( obj is IntPtr pointer )
		{
			return pointer;
		}
		else if ( obj is string str )
		{
			return Marshal.StringToCoTaskMemUTF8( str );
		}
		else if ( obj is int i )
		{
			return (IntPtr)i;
		}
		else if ( obj is float f )
		{
			return (IntPtr)f;
		}
		else if ( obj is bool b )
		{
			return b ? new IntPtr( 1 ) : IntPtr.Zero;
		}
		else
		{
			Log.Error( $"Couldn't convert {obj} to pointer (type {obj.GetType()})" );
		}

		return IntPtr.Zero;
	}

	internal static string GetString( IntPtr strPtr )
	{
		return Marshal.PtrToStringUTF8( strPtr );
	}
}
