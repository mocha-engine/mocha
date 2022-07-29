global using static Mocha.Engine.Global;
global using Vector4 = System.Numerics.Vector4;
using System.Runtime.InteropServices;

namespace Mocha.Engine;

/// <summary>
/// Program entry point
/// </summary>
public class Program
{
	public static void Main( string[] args )
	{
		var game = new Game();
	}

	[UnmanagedCallersOnly]
	public static void HostedMain()
	{
		Log.Trace( Directory.GetCurrentDirectory() );
		try
		{
			var game = new Game();
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"Caught unhandled exception:\n{ex}" );
		}
	}

	[UnmanagedCallersOnly]
	public static void CustomEntryPointUnmanaged( int number )
	{
		Console.WriteLine( "Hello world!" );
		Console.WriteLine( number );
	}
}
