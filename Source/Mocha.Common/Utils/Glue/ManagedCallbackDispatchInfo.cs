using System.Runtime.InteropServices;

namespace Mocha.Common;

[StructLayout( LayoutKind.Sequential )]
public struct ManagedCallbackDispatchInfo
{
	public uint handle;
	public int argsSize;
	public IntPtr args;
};
