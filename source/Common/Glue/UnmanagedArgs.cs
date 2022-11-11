using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __CLogger_CreateMethodPtr;
    public IntPtr __CLogger_InfoMethodPtr;
    public IntPtr __CLogger_WarningMethodPtr;
    public IntPtr __CLogger_ErrorMethodPtr;
    public IntPtr __CLogger_TraceMethodPtr;
}
