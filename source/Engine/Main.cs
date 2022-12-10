using System.Runtime.InteropServices;

namespace Mocha;

public class Main
{
	private static World world;

	private static void SetupFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new();
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
		LastUpdate = DateTime.Now;
	}

	private static DateTime LastUpdate;

	[UnmanagedCallersOnly]
	public static void Render()
	{
		Time.UpdateFrom( Glue.Entities.GetDeltaTime() );

		world.Update();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		Editor.Draw();
	}

	public delegate void FireEventDelegate( IntPtr ptrEventName );

	[UnmanagedCallersOnly]
	public static void FireEvent( IntPtr args, int sizeBytes )
	{
		var eventName = Marshal.PtrToStringUTF8( args );

		if ( eventName == null )
			return;

		Event.Run( eventName );
	}
}
