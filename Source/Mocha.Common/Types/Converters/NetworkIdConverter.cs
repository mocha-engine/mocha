using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mocha.Common;

public class NetworkIdConverter : JsonConverter<NetworkId>
{
	public override NetworkId Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		ulong value = reader.GetUInt64();
		return new( value );
	}

	public override void Write( Utf8JsonWriter writer, NetworkId networkId, JsonSerializerOptions options )
	{
		writer.WriteNumberValue( networkId.Value );
	}
}
