using StbImageSharp;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;

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
		var destFileName = Path.ChangeExtension( path, "mtex" );
		Console.WriteLine( $"Compiling {path}" );

		using var fileStream = new FileStream( destFileName, FileMode.Create );
		using var binaryWriter = new BinaryWriter( fileStream );

		binaryWriter.Write( new char[] { 'M', 'T', 'E', 'X' } ); // Magic number

		//
		// File header
		//
		binaryWriter.Write( 1 ); // Version major
		binaryWriter.Write( 2 ); // Version minor

		var fileData = File.ReadAllBytes( path );
		var image = ImageResult.FromMemory( fileData, ColorComponents.RedGreenBlueAlpha );

		var width = (uint)image.Width;
		var height = (uint)image.Height;

		binaryWriter.Write( width ); // Image width
		binaryWriter.Write( height ); // Image height

		binaryWriter.Write( new char[] { 'D', 'A', 'T', 'A' } );

		var data = image.Data;
		data = BlockCompression( data, (int)width, (int)height );

		var compressedData = Compressor.Compress( data );

		binaryWriter.Write( compressedData.Length );
		binaryWriter.Write( compressedData ); // Image data

		return destFileName;
	}
}
