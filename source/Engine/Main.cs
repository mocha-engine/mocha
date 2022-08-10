using System.Runtime.InteropServices;

namespace Mocha;

public class Main
{
	private static Editor.Editor editor;

	private static void SetupFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new Glue.CLogger();
	}

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		SetupFunctionPointers( args );

		editor = new();
		var world = new World();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		editor.Render();
	}
}
