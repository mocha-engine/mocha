using System.IO.Compression;
using System.Text.Json;

namespace Mocha.Common;

public static class Serializer
{
	private static JsonSerializerOptions CreateSerializerOptions()
	{
		var serializeOptions = new JsonSerializerOptions
		{
			WriteIndented = false
		};

		return serializeOptions;
	}

	public static byte[] Serialize<T>( T obj )
	{
		var serialized = JsonSerializer.SerializeToUtf8Bytes( obj, CreateSerializerOptions() );

		return Compress( serialized );
	}

	public static T? Deserialize<T>( byte[] serialized )
	{
		using var outputStream = new MemoryStream();

		using ( var compressStream = new MemoryStream( serialized ) )
		{
			using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
			deflateStream.CopyTo( outputStream );
		}

		return JsonSerializer.Deserialize<T>( outputStream.ToArray(), CreateSerializerOptions() );
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
