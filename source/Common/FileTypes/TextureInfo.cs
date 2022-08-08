namespace Mocha.Common;

public struct TextureInfo
{
	public uint Width { get; set; }
	public uint Height { get; set; }

	public Veldrid.PixelFormat CompressionFormat { get; set; }

	public int MipCount { get; set; }
	public int[] MipDataLength { get; set; }
	public byte[][] MipData { get; set; }
}
