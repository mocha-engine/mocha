using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __CFileSystem_CreateMethodPtr;
    public IntPtr __CFileSystem_DeleteMethodPtr;
    public IntPtr __CFileSystem_DirectoryExistsMethodPtr;
    public IntPtr __CFileSystem_FileExistsMethodPtr;
    public IntPtr __CFileSystem_IsDirectoryMethodPtr;
    public IntPtr __CFileSystem_IsFileMethodPtr;
    public IntPtr __CFileSystem_GetFilesMethodPtr;
    public IntPtr __CFileSystem_GetDirectoriesMethodPtr;
    public IntPtr __CFileSystem_ReadAllTextMethodPtr;
    public IntPtr __CFileSystem_ReadAllBytesMethodPtr;
    public IntPtr __CFileSystem_WriteAllTextMethodPtr;
    public IntPtr __CLogger_CreateMethodPtr;
    public IntPtr __CLogger_InfoMethodPtr;
    public IntPtr __CLogger_WarningMethodPtr;
    public IntPtr __CLogger_ErrorMethodPtr;
    public IntPtr __CLogger_TraceMethodPtr;
    public IntPtr __CNativeWindow_CreateMethodPtr;
    public IntPtr __CNativeWindow_RunMethodPtr;
    public IntPtr __CNativeWindow_GetWindowPointerMethodPtr;
    public IntPtr __CNativeWindow_GetWindowHandleMethodPtr;
    public IntPtr __CNativeWindow_GetWindowSizeMethodPtr;
}
