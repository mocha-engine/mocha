using System.Runtime.InteropServices;

namespace Mocha;

public class Main
{
	private static World world;

	private static void SetupFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
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
		Input.Update();
		Time.UpdateFrom( Glue.Entities.GetDeltaTime() );
		Screen.UpdateFrom( Glue.Editor.GetWindowSize() );

		const float threshold = 30f;
		if ( Time.Delta > 1.0f / threshold )
		{
			Log.Warning( $"!!! Deltatime is lower than {threshold}fps: {Time.Delta}ms ({1.0f / Time.Delta}fps) !!!" );
		}

		world.Update();
	}

	[UnmanagedCallersOnly]
	public static void DrawEditor()
	{
		Editor.Editor.Draw();
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
