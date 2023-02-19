using Mocha.Networking;

namespace Mocha;
public class BaseGameServer : Server
{
	public override void OnClientConnected( ConnectedClient client )
	{
		Log.Info( $"BaseGameServer: Client {client} connected" );
	}

	public override void OnClientDisconnected( ConnectedClient client )
	{
		Log.Info( $"BaseGameServer: Client {client} disconnected" );
	}

	public override void OnMessageReceived( ConnectedClient client, byte[] data )
	{
		var message = Serializer.Deserialize<NetworkMessageWrapper<BaseNetworkMessage>>( data )!;

		if ( message.NetworkMessageType == 0 )
		{
			// ClientInputMessage
			var clientInputMessage = Serializer.Deserialize<NetworkMessageWrapper<ClientInputMessage>>( data )!;
			var clientInput = clientInputMessage.Data;

			Log.Info( $@"BaseGameServer: Client {client} sent input message:
				ViewAngles: {clientInput.ViewAnglesP}, {clientInput.ViewAnglesY}, {clientInput.ViewAnglesR}
				Direction: {clientInput.DirectionX}, {clientInput.DirectionY}, {clientInput.DirectionZ}
				Left: {clientInput.Left}
				Right: {clientInput.Right}
				Middle: {clientInput.Middle}" );
		}
		else
		{
			Log.Error( $"BaseGameServer: Unknown message type '{message.NetworkMessageType}'" );
		}
	}
}
