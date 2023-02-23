using Mocha.Common;
using System.Text;
using System.Text.Json;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
	private const bool UseCompression = true;
	private static JsonSerializerOptions s_serializerOptions = CreateJsonSerializerOptions();

	private static JsonSerializerOptions CreateJsonSerializerOptions()
	{
		var deserializeOptions = new JsonSerializerOptions();

		deserializeOptions.Converters.Add( new NetworkIdConverter() );
		deserializeOptions.Converters.Add( new RotationConverter() );
		deserializeOptions.Converters.Add( new Vector3Converter() );
		deserializeOptions.Converters.Add( new Vector2Converter() );

		deserializeOptions.WriteIndented = true;

		return deserializeOptions;
	}

	public static byte[] Serialize( object obj )
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes( obj, s_serializerOptions );

		if ( Core.IsServer )
			Log.Info( $"Serialized data as {Encoding.UTF8.GetString( bytes )}" );

		if ( UseCompression )
			return Serializer.Compress( bytes );
		else
			return bytes;
	}

	public static T Deserialize<T>( byte[] data )
	{
		if ( UseCompression )
			data = Serializer.Decompress( data );

		return JsonSerializer.Deserialize<T>( data, s_serializerOptions );
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		if ( UseCompression )
			data = Serializer.Decompress( data );

		return JsonSerializer.Deserialize( data, type, s_serializerOptions );
	}
}
