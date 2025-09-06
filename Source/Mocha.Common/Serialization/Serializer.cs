using BinaryPack;
using System.IO.Compression;

namespace Mocha.Common;

public static class Serializer
{
	public static byte[] Serialize<T>( T obj ) where T : new()
	{
		var serialized = BinaryConverter.Serialize( obj );

		return Compress( serialized );
	}

	public static T? Deserialize<T>( byte[] data ) where T : new()
	{
		var serialized = Decompress( data );

		return BinaryConverter.Deserialize<T>( serialized );
	}

	public static byte[] Compress( byte[] uncompressedData )
	{
		using var stream = new MemoryStream();
		using var deflate = new DeflateStream( stream, CompressionLevel.Fastest );

		deflate.Write( uncompressedData );
		deflate.Close();

		return stream.ToArray();
	}

	public static byte[] Decompress( byte[] compressedData )
	{
		using var outputStream = new MemoryStream();

		using ( var compressStream = new MemoryStream( compressedData ) )
		{
			using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
			deflateStream.CopyTo( outputStream );
		}

		return outputStream.ToArray();
	}
}
