global using System.ComponentModel;
global using static Mocha.Editor.Global;

global using Matrix4x4 = System.Numerics.Matrix4x4;
global using Vector4 = System.Numerics.Vector4;
global using EditorUI = Mocha.Glue.EditorUI;

using System.Runtime.InteropServices;

namespace Mocha.Editor;

public class Program
{
	private static Editor editor;

	private static void SetupFunctionPointers( IntPtr args )
	{
		Common.Global.UnmanagedArgs = Marshal.PtrToStructure<UnmanagedArgs>( args );
		Log.NativeLogger = new Glue.CLogger();
	}

	[UnmanagedCallersOnly]
	public static void Main( IntPtr args )
	{
		SetupFunctionPointers( args );

		editor = new();
	}

	[UnmanagedCallersOnly]
	public static void Render()
	{
		editor.Render();
	}
}
