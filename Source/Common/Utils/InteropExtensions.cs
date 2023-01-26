namespace Mocha.Common;

public static class InteropExtensions
{
	public static InteropArray<T> ToInterop<T>( this List<T> list )
	{
		return InteropArray<T>.FromList( list );
	}

	public static InteropArray<T> ToInterop<T>( this T[] list )
	{
		return InteropArray<T>.FromArray( list );
	}
}
