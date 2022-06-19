using System.Runtime.InteropServices;

namespace VConsoleLib;

public static class Serializer
{
	public static byte[] ToBytes<T>( T obj ) where T : struct
	{
		int size = Marshal.SizeOf( obj );
		byte[] bytes = new byte[size];
		IntPtr ptr = IntPtr.Zero;

		try
		{
			ptr = Marshal.AllocHGlobal( size );
			Marshal.StructureToPtr( obj, ptr, false );
			Marshal.Copy( ptr, bytes, 0, size );
		}
		finally
		{
			Marshal.FreeHGlobal( ptr );
		}

		return bytes;
	}
}
