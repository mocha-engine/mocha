using StbImageSharp;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using System.Security.Cryptography;

namespace Mocha.AssetCompiler;

public class TextureCompiler
{
	private static byte[] BlockCompression( byte[] data, int width, int height )
	{
		using var stream = new MemoryStream();

		BcEncoder encoder = new BcEncoder();

		encoder.OutputOptions.GenerateMipMaps = false;
		encoder.OutputOptions.Quality = CompressionQuality.Fast;
		encoder.OutputOptions.Format = CompressionFormat.Bc3;
		encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

		return encoder.EncodeToRawBytes( data, width, height, PixelFormat.Rgba32, 0, out _, out _ );
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

		// Compress data using BC
		textureFormat.Data = BlockCompression( image.Data, image.Width, image.Height );
		textureFormat.DataLength = textureFormat.Data.Length;

		// Wrapper for file
		var mochaFile = new MochaFile<TextureInfo>()
		{
			MajorVersion = 3,
			MinorVersion = 0,
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
