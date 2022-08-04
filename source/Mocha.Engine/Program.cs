global using static Mocha.Engine.Global;
global using Vector4 = System.Numerics.Vector4;
using System.Runtime.InteropServices;

namespace Mocha.Engine;

public class Program
{
	[UnmanagedCallersOnly]
	public static void HostedMain( IntPtr args )
	{
		try
		{
			var unmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
			var window = new CNativeWindow( unmanagedArgs );
			Log.NativeLogger = new CLogger( unmanagedArgs );

			Log.Info( "Info" );
			Log.Warning( "Warning" );
			Log.Error( "Error" );
			Log.Trace( "Trace" );

			window.Create( "Test Window :3", 1280, 720 );
			Log.Trace( window.GetWindowPointer() );

			window.Run();
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"Caught unhandled exception:\n{ex}" );
		}
	}
}
