global using static Mocha.Engine.Global;
global using Matrix4x4 = System.Numerics.Matrix4x4;
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
			SetupFunctionPointers( args );
			// FilesystemTest();
			WindowTest();
		}
		catch ( SEHException sex )
		{
			Console.WriteLine( $"Unhandled native exception:\n{sex}" );
		}
		catch ( Exception ex )
		{
			Console.WriteLine( $"Unhandled .NET exception:\n{ex}" );
		}
	}

	private static void WindowTest()
	{
		var window = new Window();
		window.Run();
	}

	private static void FilesystemTest()
	{
		var fs = new FileSystem();
		var text = fs.ReadAllText( "materials/dev/dev_floor.mat" );
		Log.Info( text );
	}

	private static void SetupFunctionPointers( IntPtr args )
	{
		Common.Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new Glue.CLogger();
	}
}
