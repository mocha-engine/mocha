using MessagePack;
using MessagePack.Resolvers;
using Mocha.Common;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
	private const bool UseCompression = false;

	private static IFormatterResolver s_resolver = CompositeResolver.Create(
		MochaResolver.Instance,

		// Standard resolver is last
		StandardResolver.Instance
	);

	private static MessagePackSerializerOptions s_options = new MessagePackSerializerOptions( s_resolver );

	public static byte[] Serialize( object obj )
	{
		var bytes = MessagePackSerializer.Serialize( obj, s_options );

		Log.Info( "Dump:\n" + HexDump.Dump( bytes, 8 ) );

		return UseCompression ? Serializer.Compress( bytes ) : bytes;
	}

	public static T Deserialize<T>( byte[] data )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		var obj = MessagePackSerializer.Deserialize<T>( data, s_options );
		return obj;
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		data = UseCompression ? Serializer.Decompress( data ) : data;

		var obj = MessagePackSerializer.Deserialize( type, data, s_options );
		return obj;
	}
}
