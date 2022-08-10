using System.Runtime.InteropServices;

namespace Mocha;

public class Main
{
#if DEBUG
	private static Editor.Editor editor;
#endif

	private static void SetupFunctionPointers( IntPtr args )
	{
		Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new Glue.CLogger();
	}

	[UnmanagedCallersOnly]
	public static void Run( IntPtr args )
	{
		SetupFunctionPointers( args );

#if DEBUG
		editor = new();
#endif

		var world = new World();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
#if DEBUG
		editor.Render();
#endif
	}
}
