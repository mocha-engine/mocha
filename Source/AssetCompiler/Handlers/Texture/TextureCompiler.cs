using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Mocha.Common.Serialization;
using StbImageSharp;
using System.Security.Cryptography;
using System.Text.Json;

namespace Mocha.AssetCompiler;

[Handles( new[] { ".png", ".jpg" } )]
public partial class TextureCompiler : BaseCompiler
{
	private static CompressionFormat TextureFormatToCompressionFormat( TextureFormat format )
	{
		return format switch
		{
			TextureFormat.BC3_SRGB => CompressionFormat.Bc3,
			TextureFormat.BC3_UNORM => CompressionFormat.Bc3,
			TextureFormat.BC5_UNORM => CompressionFormat.Bc5,
			TextureFormat.R8G8B8A8_SRGB => CompressionFormat.Rgba,

			_ => throw new Exception( $"Unsupported texture format {format}" ),
		};
	}

	private static byte[] BlockCompression( byte[] data, uint width, uint height, uint mip, TextureFormat format )
	{
		BcEncoder encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = true;
		encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
		encoder.OutputOptions.Format = TextureFormatToCompressionFormat( format );
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		return encoder.EncodeToRawBytes( data, (int)width, (int)height, PixelFormat.Rgba32, (int)mip, out _, out _ );
	}

	private bool IsPowerOfTwo( int x )
	{
		return (x & (x - 1)) == 0;
	}

	private int NextPowerOfTwo( int x )
	{
		int i = 0;
		while ( x > 0 )
		{
			x >>= 1;
			i++;
		}
		return 1 << i;
	}

	public override string CompileFile( string path )
	{
		Log.Processing( "Texture", path );

		var destFileName = Path.ChangeExtension( path, "mtex_c" );
		var metaFileName = Path.ChangeExtension( path, "meta" );
		var textureMeta = new TextureMetadata();
		var textureFormat = new TextureInfo();

		// Load image
		var fileData = File.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		// TODO: Move to a nice generic function somewhere
		if ( File.Exists( destFileName ) )
		{
			// Read mocha file
			var existingFile = File.ReadAllBytes( destFileName );
			var deserializedFile = Serializer.Deserialize<MochaFile<TextureInfo>>( existingFile );

			using var md5 = MD5.Create();
			var computedHash = md5.ComputeHash( fileData );
			if ( Enumerable.SequenceEqual( deserializedFile.AssetHash, computedHash ) )
			{
				Log.Skip( path );

				return destFileName;
			}
		}

		// Check for meta, load if it exists
		if ( File.Exists( metaFileName ) )
		{
			var metaFile = File.ReadAllText( metaFileName );
			textureMeta = JsonSerializer.Deserialize<TextureMetadata>( metaFile );
		}

		textureFormat.DataWidth = (uint)image.Width;
		textureFormat.DataHeight = (uint)image.Height;
		textureFormat.Width = (uint)image.Width;
		textureFormat.Height = (uint)image.Height;
		textureFormat.MipCount = 5;
		textureFormat.MipData = new byte[textureFormat.MipCount][];
		textureFormat.MipDataLength = new int[textureFormat.MipCount];
		textureFormat.Format = textureMeta.Format;

		// If image is not POT, then pad the image with transparent pixels
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

		for ( uint i = 0; i < textureFormat.MipCount; ++i )
		{
			textureFormat.MipData[i] = BlockCompression( image.Data, textureFormat.DataWidth, textureFormat.DataHeight, i, textureMeta.Format );
			textureFormat.MipDataLength[i] = textureFormat.MipData[i].Length;
		}

		// Wrapper for file
		var mochaFile = new MochaFile<TextureInfo>()
		{
			MajorVersion = 3,
			MinorVersion = 1,
			Data = textureFormat
		};

		// Calculate original asset hash
		using ( var md5 = MD5.Create() )
			mochaFile.AssetHash = md5.ComputeHash( fileData );

		// Write result
		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( Serializer.Serialize( mochaFile ) );
		return destFileName;
	}
}
