using MessagePack;
using MessagePack.Formatters;
using Mocha.Common;

namespace Mocha.Networking;

internal class RotationFormatter : IMessagePackFormatter<Rotation>
{
	public void Serialize( ref MessagePackWriter writer, Rotation value, MessagePackSerializerOptions options )
	{
		writer.WriteArrayHeader( 4 );
		writer.Write( value.X );
		writer.Write( value.Y );
		writer.Write( value.Z );
		writer.Write( value.W );
	}

	public Rotation Deserialize( ref MessagePackReader reader, MessagePackSerializerOptions options )
	{
		var length = reader.ReadArrayHeader();
		if ( length != 4 )
		{
			throw new MessagePackSerializationException( $"Expected array of length 4, got {length}." );
		}

		var x = reader.ReadSingle();
		var y = reader.ReadSingle();
		var z = reader.ReadSingle();
		var w = reader.ReadSingle();

		return new Rotation( x, y, z, w );
	}
}
