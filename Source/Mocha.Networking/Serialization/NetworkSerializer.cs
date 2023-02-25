using MessagePack;
using Mocha.Common;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
	private const bool UseCompression = false;

	public static byte[] Serialize( object obj )
	{
		var bytes = MessagePackSerializer.Serialize( obj );

		Log.Info( "Dump:\n" + HexDump.Dump( bytes, 8 ) );

		return UseCompression ? Serializer.Compress( bytes ) : bytes;
	}

	public static T Deserialize<T>( byte[] data )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		var obj = MessagePackSerializer.Deserialize<T>( data );
		return obj;
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		var obj = MessagePackSerializer.Deserialize( type, data );
		return obj;
	}
}
