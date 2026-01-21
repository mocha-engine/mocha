using System.Runtime.InteropServices;

namespace Mocha.Networking;

[StructLayout( LayoutKind.Sequential )]
struct ValveSocketReceivedMessage
{
	public IntPtr connectionHandle;
	public int size;
	public IntPtr data;
};
