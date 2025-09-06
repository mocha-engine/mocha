namespace Mocha.Common;

/// <summary>
/// This contains the information required to load a Mocha texture.
/// </summary>
/// <remarks>
/// Padding is applied to non power-of-two textures in order to make them
/// power-of-two textures.
/// </remarks>
public struct TextureInfo
{
	/// <summary>
	/// The width of the texture, sans padding (see <see cref="TextureInfo"/> remarks).
	/// </summary>
	public uint Width { get; set; }

	/// <summary>
	/// The height of the texture, sans padding (see <see cref="TextureInfo"/> remarks).
	/// </summary>
	public uint Height { get; set; }

	/// <summary>
	/// The width of the texture including any padding (see <see cref="TextureInfo"/> remarks).
	/// </summary>
	public uint DataWidth { get; set; }

	/// <summary>
	/// The height of the texture including any padding (see <see cref="TextureInfo"/> remarks).
	/// </summary>
	public uint DataHeight { get; set; }

	/// <summary>
	/// The number of mipmaps in the texture.
	/// </summary>
	public int MipCount { get; set; }

	/// <summary>
	/// The length of each mipmap in bytes.
	/// </summary>
	public int[] MipDataLength { get; set; }

	/// <summary>
	/// The data for each mipmap.
	/// </summary>
	public byte[][] MipData { get; set; }

	/// <summary>
	/// The format for the data in the texture.
	/// </summary>
	public TextureFormat Format { get; set; }
}
