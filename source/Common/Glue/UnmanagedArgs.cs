using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __LogManager_CtorMethodPtr;
	public IntPtr __LogManager_StartUpMethodPtr;
	public IntPtr __LogManager_ShutDownMethodPtr;
	public IntPtr __LogManager_InfoMethodPtr;
	public IntPtr __LogManager_WarningMethodPtr;
	public IntPtr __LogManager_ErrorMethodPtr;
	public IntPtr __LogManager_TraceMethodPtr;
	public IntPtr __Camera_CtorMethodPtr;
	public IntPtr __Camera_SetPositionMethodPtr;
	public IntPtr __Camera_UpdateMethodPtr;
	public IntPtr __ManagedModel_SetIndexDataMethodPtr;
	public IntPtr __ManagedModel_SetVertexDataMethodPtr;
	public IntPtr __ManagedModel_FinishMethodPtr;
	public IntPtr __ManagedModel_CtorMethodPtr;
	public IntPtr __Shader_CtorMethodPtr;
}
