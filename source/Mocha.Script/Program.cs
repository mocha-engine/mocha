using System;
using System.Runtime.InteropServices;

namespace Mocha.Script
{
	public static class Program
	{
		[UnmanagedCallersOnly]
		public static void CustomEntryPointUnmanaged( int number )
		{
			Console.WriteLine( "Hello world!" );
			Console.WriteLine( number );
		}
	}
}
