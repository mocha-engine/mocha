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
		else if ( obj is IInteropArray arr )
		{
			return GetPtr( arr.GetNative() );
		}
		else if ( obj is INativeGlue native )
		{
			return native.NativePtr;
		}
		else if ( obj is string str )
		{
			return Marshal.StringToCoTaskMemUTF8( str );
		}
		else if ( obj is int i )
		{
			return (IntPtr)i;
		}
		else if ( obj is uint u )
		{
			return (IntPtr)u;
		}
		else if ( obj is float f )
		{
			return (IntPtr)f;
		}
		else if ( obj is bool b )
		{
			return b ? new IntPtr( 1 ) : IntPtr.Zero;
		}
		else if ( obj.GetType().IsValueType )
		{
			var ptr = Marshal.AllocHGlobal( Marshal.SizeOf( obj ) );
			Marshal.StructureToPtr( obj, ptr, false );
			return ptr;
		}
		else
		{
			Log.Error( $"Couldn't convert {obj} to pointer (type {obj.GetType()})" );
		}

		return IntPtr.Zero;
	}

	internal static string GetString( IntPtr strPtr )
	{
		return Marshal.PtrToStringUTF8( strPtr ) ?? "UNKNOWN";
	}
}

public interface IInteropArray
{
	public Glue.InteropArray GetNative();
}

public class InteropArray<T> : IInteropArray
{
	private Glue.InteropArray NativeStruct;
	private InteropArray() { }

	public static InteropArray<T> FromArray( T[] array )
	{
		bool isNativeGlue = typeof( T ).GetInterfaces().Contains( typeof( INativeGlue ) );

		int stride, size;

		var s = new InteropArray<T>();
		s.NativeStruct = new();

		if ( isNativeGlue )
			stride = Marshal.SizeOf( typeof( IntPtr ) );
		else
			stride = Marshal.SizeOf( typeof( T ) );

		size = stride * array.Length;

		s.NativeStruct.count = array.Length;
		s.NativeStruct.size = size;

		unsafe
		{
			if ( isNativeGlue )
			{
				fixed ( void* data = array.Select( x => (x as INativeGlue).NativePtr ).ToArray() )
					s.NativeStruct.data = (IntPtr)data;
			}
			else
			{
				fixed ( void* data = array )
					s.NativeStruct.data = (IntPtr)data;
			}
		}

		return s;
	}

	public static InteropArray<T> FromList( List<T> list )
	{
		return FromArray( list.ToArray() );
	}

	public Glue.InteropArray GetNative()
	{
		return NativeStruct;
	}

	//
	// Implicit conversions to Glue.InteropStruct and from lists/arrays
	//
	public static implicit operator Glue.InteropArray( InteropArray<T> arr ) => arr.GetNative();
	public static implicit operator InteropArray<T>( List<T> list ) => FromList( list );
}
