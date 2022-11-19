using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __Logger_CtorMethodPtr;
	public IntPtr __Logger_InfoMethodPtr;
	public IntPtr __Logger_WarningMethodPtr;
	public IntPtr __Logger_ErrorMethodPtr;
	public IntPtr __Logger_TraceMethodPtr;
	public IntPtr __Camera_CtorMethodPtr;
	public IntPtr __Camera_SetPositionMethodPtr;
	public IntPtr __Camera_UpdateMethodPtr;
	public IntPtr __ManagedModel_SetIndexDataMethodPtr;
	public IntPtr __ManagedModel_SetVertexDataMethodPtr;
	public IntPtr __ManagedModel_FinishMethodPtr;
	public IntPtr __ManagedModel_CtorMethodPtr;
	public IntPtr __Shader_CtorMethodPtr;
}
