using StbImageSharp;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using System.Security.Cryptography;
using System.Runtime.ExceptionServices;

namespace Mocha.AssetCompiler;

public class TextureCompiler
{
	private static byte[] BlockCompression( byte[] data, int width, int height, int mip, CompressionFormat compressionFormat )
	{
		BcEncoder encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = true;
		encoder.OutputOptions.Quality = CompressionQuality.Fast;
		encoder.OutputOptions.Format = compressionFormat;
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		return encoder.EncodeToRawBytes( data, width, height, PixelFormat.Rgba32, mip, out _, out _ );
	}

	public static string CompileFile( string path )
	{
		Console.WriteLine( $"[TEXTURE]\t{path}" );

		var destFileName = Path.ChangeExtension( path, "mtex" );
		var textureFormat = new TextureInfo();

		// Load image
		var fileData = File.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		textureFormat.Width = (uint)image.Width;
		textureFormat.Height = (uint)image.Height;
		textureFormat.MipCount = 5;
		textureFormat.MipData = new byte[textureFormat.MipCount][];
		textureFormat.MipDataLength = new int[textureFormat.MipCount];

		// Change compression format based on normal map
		for ( int i = 0; i < textureFormat.MipCount; ++i )
		{
			if ( path.Contains( "Normal" ) )
			{
				// Do not compress
				textureFormat.CompressionFormat = Veldrid.PixelFormat.BC5_UNorm;
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Bc5 );
			}
			else
			{
				textureFormat.CompressionFormat = Veldrid.PixelFormat.BC3_UNorm;
				textureFormat.MipData[i] = BlockCompression( image.Data, image.Width, image.Height, i, CompressionFormat.Bc3 );
			}

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
