using BinaryPack;
using System.IO.Compression;

namespace Mocha.Common;

public static class Serializer
{
	public static byte[] Serialize<T>( T obj ) where T : new()
	{
		var serialized = BinaryConverter.Serialize( obj );

		if ( serialized.Length < 512 )
		{
			return serialized;
		}

		var header = "BILZ"u8.ToArray();
		return header.Concat( Compress( serialized ) ).ToArray();
	}

	public static T? Deserialize<T>( byte[] data ) where T : new()
	{
		byte[] decompressedData = data;

		if ( data.Length > 4 )
		{
			var header = data[0..4];

			if ( Enumerable.SequenceEqual( header, "BILZ"u8.ToArray() ) )
				decompressedData = Decompress( data[4..] );
		}

		return BinaryConverter.Deserialize<T>( decompressedData );
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
