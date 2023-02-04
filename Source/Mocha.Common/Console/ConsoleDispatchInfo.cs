using System.Runtime.InteropServices;

namespace Mocha.Common.Console;


[StructLayout( LayoutKind.Sequential )]
public struct ConCmdDispatchInfo
{
	public IntPtr name;
	public IntPtr data;
	public int size;
}

[StructLayout( LayoutKind.Sequential )]
public struct StringCVarDispatchInfo
{
	public IntPtr name;
	public IntPtr oldValue;
	public IntPtr newValue;
}

[StructLayout( LayoutKind.Sequential )]
public struct FloatCVarDispatchInfo
{
	public IntPtr name;
	public float oldValue;
	public float newValue;
}

[StructLayout( LayoutKind.Sequential )]
public struct BoolCVarDispatchInfo
{
	public IntPtr name;
	public bool oldValue;
	public bool newValue;
}
