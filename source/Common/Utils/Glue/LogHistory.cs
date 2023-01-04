using System.Runtime.InteropServices;

namespace Mocha.Glue;

[StructLayout( LayoutKind.Sequential )]
public struct LogHistory
{
	public int count;
	public IntPtr items;
}
