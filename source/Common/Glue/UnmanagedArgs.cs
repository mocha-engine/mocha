using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __Entities_CreateBaseEntityMethodPtr;
	public IntPtr __Entities_CreateModelEntityMethodPtr;
	public IntPtr __Entities_SetPositionMethodPtr;
	public IntPtr __Entities_SetRotationMethodPtr;
	public IntPtr __Entities_SetScaleMethodPtr;
	public IntPtr __Entities_SetNameMethodPtr;
	public IntPtr __Entities_GetPositionMethodPtr;
	public IntPtr __Entities_GetRotationMethodPtr;
	public IntPtr __Entities_GetScaleMethodPtr;
	public IntPtr __Entities_GetNameMethodPtr;
	public IntPtr __Entities_SetModelMethodPtr;
	public IntPtr __Entities_SetCameraPositionMethodPtr;
	public IntPtr __LogManager_CtorMethodPtr;
	public IntPtr __LogManager_StartupMethodPtr;
	public IntPtr __LogManager_ShutdownMethodPtr;
	public IntPtr __LogManager_InfoMethodPtr;
	public IntPtr __LogManager_WarningMethodPtr;
	public IntPtr __LogManager_ErrorMethodPtr;
	public IntPtr __LogManager_TraceMethodPtr;
	public IntPtr __ManagedTexture_SetDataMethodPtr;
	public IntPtr __ManagedTexture_CtorMethodPtr;
	public IntPtr __Camera_CtorMethodPtr;
	public IntPtr __ManagedModel_SetIndexDataMethodPtr;
	public IntPtr __ManagedModel_SetVertexDataMethodPtr;
	public IntPtr __ManagedModel_FinishMethodPtr;
	public IntPtr __ManagedModel_CtorMethodPtr;
	public IntPtr __Shader_CtorMethodPtr;
}
