using System.IO.Compression;

namespace Mocha.AssetCompiler;

public static class Compressor
{
	public static byte[] Compress( byte[] data )
	{
		return data;
		using var stream = new MemoryStream();
		using var deflate = new DeflateStream( stream, CompressionLevel.Optimal );

		deflate.Write( data );
		deflate.Close();

		return stream.ToArray();
	}

	public static byte[] Decompress( byte[] bytes )
	{
		return bytes;
		using var outputStream = new MemoryStream();

		using ( var compressStream = new MemoryStream( bytes ) )
		{
			using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
			deflateStream.CopyTo( outputStream );
		}

		return outputStream.ToArray();
	}
}
