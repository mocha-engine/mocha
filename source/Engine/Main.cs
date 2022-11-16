using System.Runtime.InteropServices;

namespace Mocha;

public class Main
{
	private static World world;

	private static void SetupFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new Glue.CLogger();
	}

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		SetupFunctionPointers( args );
		Log.Info( "Managed init" );

		// Get parent process path
		var parentProcess = System.Diagnostics.Process.GetCurrentProcess();
		var parentModule = parentProcess.MainModule;
		var parentPath = parentModule?.FileName ?? "None";
		Log.Info( $"Parent process: {parentPath}" );

		world = new World();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		world.Render();
	}
}
