using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mocha.Common;

public class Vector2Converter : JsonConverter<Vector2>
{
	public override Vector2 Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		// Read a JSON object with two properties: X and Y
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

		// Skip the end object token
		reader.Read();

		return new Vector2 { X = x, Y = y };
	}

	public override void Write( Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options )
	{
		// Write a JSON object with two properties: X and Y
		writer.WriteStartObject();
		writer.WriteNumber( "X", value.X );
		writer.WriteNumber( "Y", value.Y );
		writer.WriteEndObject();
	}
}
