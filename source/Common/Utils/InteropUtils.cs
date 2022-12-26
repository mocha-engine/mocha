using System.Runtime.InteropServices;

namespace Mocha.Glue;

public static class MemoryLogger
{
	public static Dictionary<string, (int Allocations, int Frees)> Entries = new();

	public static void AllocatedBytes( string name, int count )
	{
		if ( !Entries.ContainsKey( name ) )
			Entries[name] = (0, 0);

		Entries[name] = (Entries[name].Allocations + count, Entries[name].Frees);
	}

	public static void FreedBytes( string name, int count )
	{
		if ( !Entries.ContainsKey( name ) )
			Entries[name] = (0, 0);

		Entries[name] = (Entries[name].Allocations, Entries[name].Frees + count);
	}
}

/// <summary>
/// Use this for allocating memory, it will automatically free it
/// when IDisposable.Dispose is called.
/// </summary>
public class MemoryContext : IDisposable
{
	enum Type
	{
		CoTaskMem,
		HGlobal
	}

	private List<(Type Type, IntPtr Pointer)> Values { get; } = new();

	private string Name { get; }

	public MemoryContext( string name )
	{
		Name = name;
	}

	public IntPtr StringToCoTaskMemUTF8( string str )
	{
		var ptr = Marshal.StringToCoTaskMemUTF8( str );
		Values.Add( (Type.CoTaskMem, ptr) );

		MemoryLogger.AllocatedBytes( Name, IntPtr.Size );

		return ptr;
	}

	public IntPtr AllocHGlobal( int size )
	{
		var ptr = Marshal.AllocHGlobal( size );
		Values.Add( (Type.HGlobal, ptr) );

		MemoryLogger.AllocatedBytes( Name, IntPtr.Size );

		return ptr;
	}

	public void Dispose()
	{
		foreach ( var value in Values )
		{
			switch ( value.Type )
			{
				case Type.CoTaskMem:
					Marshal.FreeCoTaskMem( value.Pointer );
					break;
				case Type.HGlobal:
					Marshal.FreeHGlobal( value.Pointer );
					break;
			}
		}

		MemoryLogger.FreedBytes( Name, Values.Count * IntPtr.Size );
	}

	public IntPtr GetPtr( object obj )
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
			return StringToCoTaskMemUTF8( str );
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
		else if ( obj is Enum e )
		{
			return GetPtr( Convert.ToInt32( e ) );
		}
		else if ( obj.GetType().IsValueType )
		{
			var ptr = AllocHGlobal( Marshal.SizeOf( obj ) );
			Marshal.StructureToPtr( obj, ptr, false );
			return ptr;
		}
		else
		{
			Log.Error( $"Couldn't convert {obj} to pointer (type {obj.GetType()})" );
		}

		return IntPtr.Zero;
	}

	internal string GetString( IntPtr strPtr )
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
