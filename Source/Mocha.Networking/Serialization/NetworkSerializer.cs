using Mocha.Common;
using System.Text;
using System.Text.Json;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
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

	private static JsonSerializerOptions SerializerOptions = CreateJsonSerializerOptions();

	public static byte[] Serialize( object obj )
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes( obj, SerializerOptions );

		if ( Core.IsServer )
			Log.Info( $"Serialized data as {Encoding.UTF8.GetString( bytes )}" );

		return bytes;
	}

	public static T Deserialize<T>( byte[] data )
	{
		return JsonSerializer.Deserialize<T>( data, SerializerOptions );
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		return JsonSerializer.Deserialize( data, type, SerializerOptions );
	}
}
