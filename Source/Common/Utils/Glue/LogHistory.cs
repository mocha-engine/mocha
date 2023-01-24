using System.Runtime.InteropServices;

namespace Mocha.Common;

[StructLayout( LayoutKind.Sequential )]
public struct LogHistory
{
	public int count;
	public IntPtr items;
}
