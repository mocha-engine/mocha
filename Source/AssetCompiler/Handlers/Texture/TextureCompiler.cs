using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using StbImageSharp;
using System.Diagnostics;
using System.Text.Json;

namespace MochaTool.AssetCompiler;

/// <summary>
/// A compiler for .png, .jpg, and .jpeg image files.
/// </summary>
[Handles( ".png", ".jpg", ".jpeg" )]
public partial class TextureCompiler : BaseCompiler
{
	/// <inheritdoc/>
	public override string AssetName => "Texture";

	/// <inheritdoc/>
	public override string CompiledExtension => "mtex_c";

	/// <inheritdoc/>
	public override bool SupportsMochaFile => true;

	/// <inheritdoc/>
	public override string[] AssociatedFiles => s_associatedFiles;
	private static readonly string[] s_associatedFiles = new string[]
	{
		"{SourcePathWithoutExt}.meta"
	};

	/// <inheritdoc/>
	public override CompileResult Compile( ref CompileInput input )
	{
		var textureMeta = new TextureMetadata();
		// Check for meta, load if it exists.
		if ( input.AssociatedData.TryGetValue( "{SourcePathWithoutExt}.meta", out var metaData ) )
			textureMeta = JsonSerializer.Deserialize<TextureMetadata>( metaData.Span );

		// Setup image and format.
		var image = ImageResult.FromMemory( input.SourceData.ToArray(), ColorComponents.RedGreenBlueAlpha );
		var textureFormat = new TextureInfo
		{
			DataWidth = (uint)image.Width,
			DataHeight = (uint)image.Height,
			Width = (uint)image.Width,
			Height = (uint)image.Height,
			MipCount = 5,
			Format = textureMeta.Format
		};
		textureFormat.MipData = new byte[textureFormat.MipCount][];
		textureFormat.MipDataLength = new int[textureFormat.MipCount];

		// If image is not POT, then pad the image with transparent pixels.
		if ( !IsPowerOfTwo( image.Width ) || !IsPowerOfTwo( image.Height ) )
		{
			var newWidth = NextPowerOfTwo( image.Width );
			var newHeight = NextPowerOfTwo( image.Height );

			var newData = new byte[newWidth * newHeight * 4];
			for ( var y = 0; y < newHeight; y++ )
			{
				for ( var x = 0; x < newWidth; x++ )
				{
					var index = (y * newWidth + x) * 4;
					if ( x < image.Width && y < image.Height )
					{
						var origIndex = (y * image.Width + x) * 4;

						newData[index + 0] = image.Data[origIndex + 0];
						newData[index + 1] = image.Data[origIndex + 1];
						newData[index + 2] = image.Data[origIndex + 2];
						newData[index + 3] = image.Data[origIndex + 3];
					}
					else
					{
						newData[index + 0] = 0;
						newData[index + 1] = 0;
						newData[index + 2] = 0;
						newData[index + 3] = 0;
					}
				}
			}

			image.Data = newData;
			textureFormat.DataWidth = (uint)newWidth;
			textureFormat.DataHeight = (uint)newHeight;
		}

		// Setup mip-maps.
		for ( uint i = 0; i < textureFormat.MipCount; ++i )
		{
			textureFormat.MipData[i] = BlockCompression( image.Data, textureFormat.DataWidth, textureFormat.DataHeight, i, textureMeta.Format );
			textureFormat.MipDataLength[i] = textureFormat.MipData[i].Length;
		}

		// Wrapper for file.
		var mochaFile = new MochaFile<TextureInfo>
		{
			MajorVersion = 4,
			MinorVersion = 0,
			Data = textureFormat,
			AssetHash = input.DataHash
		};

		return Succeeded( Serializer.Serialize( mochaFile ) );
	}

	private static CompressionFormat TextureFormatToCompressionFormat( TextureFormat format )
	{
		return format switch
		{
			TextureFormat.BC3 => CompressionFormat.Bc3,
			TextureFormat.BC5 => CompressionFormat.Bc5,
			TextureFormat.RGBA => CompressionFormat.Rgba,

			_ => throw new UnreachableException( $"Unsupported texture format {format}" ),
		};
	}

	private static byte[] BlockCompression( byte[] data, uint width, uint height, uint mip, TextureFormat format )
	{
		var encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = true;
		encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
		encoder.OutputOptions.Format = TextureFormatToCompressionFormat( format );
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		return encoder.EncodeToRawBytes( data, (int)width, (int)height, PixelFormat.Rgba32, (int)mip, out _, out _ );
	}

	private static bool IsPowerOfTwo( int x )
	{
		return (x & (x - 1)) == 0;
	}

	private static int NextPowerOfTwo( int x )
	{
		int i = 0;
		while ( x > 0 )
		{
			x >>= 1;
			i++;
		}

		return 1 << i;
	}
}
