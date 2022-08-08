using System.Runtime.InteropServices;

namespace Mocha.Renderer;

[StructLayout( LayoutKind.Sequential )]
public struct EmptyUniformBuffer
{
	/*
	 * These fields are padded so that they're
	 * aligned (as blocks) to multiples of 16.
	 */

	public float padding0;
	public float padding1;
	public float padding2;
	public float padding3;
}
