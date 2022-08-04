using System.Runtime.InteropServices;

// TODO(AG): InteropGen this
[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
	public IntPtr CNativeWindowPtr;
	public IntPtr CLoggerPtr;

	public IntPtr CreateMethodPtr;
	public IntPtr RunMethodPtr;
	public IntPtr GetWindowPointerMethodPtr;

	public IntPtr InfoMethodPtr;
	public IntPtr WarningMethodPtr;
	public IntPtr ErrorMethodPtr;
	public IntPtr TraceMethodPtr;
}
