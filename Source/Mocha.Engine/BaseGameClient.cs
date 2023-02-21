using Mocha.Networking;
using System.Text.Json;

namespace Mocha;
public class BaseGameClient : Client
{
	public BaseGameClient( string ipAddress, ushort port = 10570 ) : base( ipAddress, port )
	{
	}

	public override void OnMessageReceived( byte[] data )
	{
		var message = JsonSerializer.Deserialize<NetworkMessageWrapper<object>>( data )!;

		if ( message.NetworkMessageType == KickedMessage.MessageId )
		{
			// KickedMessage
			var kickedMessage = JsonSerializer.Deserialize<NetworkMessageWrapper<KickedMessage>>( data )!;
			Log.Info( $"BaseGameClient: We were kicked: '{kickedMessage.Data.Reason}'" );
		}
		else
		{
			Log.Error( $"BaseGameClient: Unknown message type '{message.NetworkMessageType}'" );
		}
	}
}
