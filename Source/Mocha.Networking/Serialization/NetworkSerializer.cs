using Mocha.Common;
using System.Text.Json;

namespace Mocha.Networking;

internal static class NetworkSerializer
{
	private static JsonSerializerOptions CreateJsonSerializerOptions()
	{
		var deserializeOptions = new JsonSerializerOptions();
		deserializeOptions.Converters.Add( new NetworkIdConverter() );

		return deserializeOptions;
	}

	public static byte[] Serialize( object obj )
	{
		return JsonSerializer.SerializeToUtf8Bytes( obj, CreateJsonSerializerOptions() );
	}

	public static T Deserialize<T>( byte[] data )
	{
		return JsonSerializer.Deserialize<T>( data, CreateJsonSerializerOptions() );
	}

	public static object? Deserialize( byte[] data, Type type )
	{
		return JsonSerializer.Deserialize( data, type, CreateJsonSerializerOptions() );
	}
}
