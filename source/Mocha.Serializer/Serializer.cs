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

	public static byte[] Serialize<T>( MochaFile<T> obj )
	{
		using var stream = new MemoryStream();
		using var deflate = new DeflateStream( stream, CompressionLevel.Fastest );

		var serialized = JsonSerializer.SerializeToUtf8Bytes( obj, CreateSerializerOptions() );

		deflate.Write( serialized );
		deflate.Close();

		return stream.ToArray();
	}

	public static MochaFile<T> Deserialize<T>( byte[] serialized )
	{
		using var outputStream = new MemoryStream();

		using ( var compressStream = new MemoryStream( serialized ) )
		{
			using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
			deflateStream.CopyTo( outputStream );
		}

		return JsonSerializer.Deserialize<MochaFile<T>>( outputStream.ToArray(), CreateSerializerOptions() );
	}
}
