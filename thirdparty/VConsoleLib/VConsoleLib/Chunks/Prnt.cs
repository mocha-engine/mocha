using System.Runtime.InteropServices;

namespace VConsoleLib;

[StructLayout( LayoutKind.Sequential )]
internal struct Prnt
{
	public uint Channel = 0;
	public uint _Padding0 = 0;
	public uint _Padding1 = 0;
	public uint Color = 0;
	public uint _Unknown0 = 0;
	public uint _Padding2 = 0;
	public uint _Padding3 = 0;

	[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
	public string String = "";

	uint SwapEndianness( uint x )
	{
		return ((x & 0x000000ff) << 24) +  // First byte
			   ((x & 0x0000ff00) << 8) +   // Second byte
			   ((x & 0x00ff0000) >> 8) +   // Third byte
			   ((x & 0xff000000) >> 24);   // Fourth byte
	}

	public Prnt( string channel, string str, uint color = 0xFE71DCFF )
	{
		this.Channel = Chan.ChanEntry.CalcChannelID( channel );
		this.String = str;
		this.Color = color;
	}
}
