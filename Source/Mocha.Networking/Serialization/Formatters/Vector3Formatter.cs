using MessagePack;
using MessagePack.Formatters;
using Mocha.Common;

namespace Mocha.Networking;

internal class Vector3Formatter : IMessagePackFormatter<Vector3>
{
	public void Serialize( ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options )
	{
		writer.WriteArrayHeader( 3 );
		writer.Write( value.X );
		writer.Write( value.Y );
		writer.Write( value.Z );
	}

	public Vector3 Deserialize( ref MessagePackReader reader, MessagePackSerializerOptions options )
	{
		var length = reader.ReadArrayHeader();
		if ( length != 3 )
		{
			throw new MessagePackSerializationException( $"Expected array of length 3, got {length}." );
		}

		var x = reader.ReadSingle();
		var y = reader.ReadSingle();
		var z = reader.ReadSingle();

		return new Vector3( x, y, z );
	}
}
