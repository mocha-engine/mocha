using System.Runtime.InteropServices;
using System.Text;

namespace VConsoleLib;

[StructLayout( LayoutKind.Sequential )]
internal struct AInf
{
	public uint _Unknown0 = 0x464126AF;
	public ushort _Padding0 = 0;
	public uint _Padding1 = 0;

	[MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
	public byte[] ExecutableName = new byte[32];

	[MarshalAs( UnmanagedType.ByValArray, SizeConst = 32 )]
	public byte[] AppName = new byte[32];

	public uint _Unknown1 = 0x00000000;
	public uint _Unknown2 = 0x00000000;

	public byte CommandLineLength = 0;

	[MarshalAs( UnmanagedType.ByValTStr, SizeConst = 128 )]
	public string CommandLine = "";

	static byte[] StringToByteArray( string str, int length )
	{
		return Encoding.ASCII.GetBytes( str.PadRight( length, '\0' ) );
	}

	public AInf( string gameName, string commandLine )
	{
		this.ExecutableName = StringToByteArray( gameName, 32 );
		this.AppName = StringToByteArray( gameName, 32 );
		this.CommandLine = commandLine;

		this.CommandLineLength = (byte)CommandLine.Length;
	}
}
