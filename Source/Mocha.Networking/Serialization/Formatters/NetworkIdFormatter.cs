using MessagePack;
using MessagePack.Formatters;
using Mocha.Common;

namespace Mocha.Networking;

internal class NetworkIdFormatter : IMessagePackFormatter<NetworkId>
{
	public void Serialize( ref MessagePackWriter writer, NetworkId value, MessagePackSerializerOptions options )
	{
		writer.Write( value.Value );
	}

	public NetworkId Deserialize( ref MessagePackReader reader, MessagePackSerializerOptions options )
	{
		return new NetworkId( reader.ReadUInt64() );
	}
}
