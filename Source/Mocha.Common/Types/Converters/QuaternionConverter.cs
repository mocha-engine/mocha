using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mocha.Common;

public class RotationConverter : JsonConverter<Rotation>
{
	public override Rotation Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		// Read a JSON object with three properties: X, Y, Z, and W
		if ( reader.TokenType != JsonTokenType.StartObject )
			throw new JsonException();

		reader.Read();
		if ( reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "X" )
			throw new JsonException();

		reader.Read();
		var x = reader.GetSingle();

		reader.Read();
		if ( reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Y" )
			throw new JsonException();

		reader.Read();
		var y = reader.GetSingle();

		reader.Read();
		if ( reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Z" )
			throw new JsonException();

		reader.Read();
		var z = reader.GetSingle();

		reader.Read();
		if ( reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "W" )
			throw new JsonException();

		reader.Read();
		var w = reader.GetSingle();

		// Skip the end object token
		reader.Read();

		return new Rotation { X = x, Y = y, Z = z, W = w };
	}

	public override void Write( Utf8JsonWriter writer, Rotation value, JsonSerializerOptions options )
	{
		// Write a JSON object with three properties: X, Y and Z
		writer.WriteStartObject();
		writer.WriteNumber( "X", value.X );
		writer.WriteNumber( "Y", value.Y );
		writer.WriteNumber( "Z", value.Z );
		writer.WriteNumber( "W", value.W );
		writer.WriteEndObject();
	}
}
