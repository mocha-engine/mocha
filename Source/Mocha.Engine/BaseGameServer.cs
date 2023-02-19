using Mocha.Networking;
using System.Text;

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
		Log.Info( $"BaseGameServer: Received message '{Encoding.ASCII.GetString( data )}' from client {client}" );

		var message = Serializer.Deserialize<NetworkMessage<BaseNetworkMessage>>( data )!;

		switch ( message.NetworkMessageType )
		{
			case 0:
				// ClientInputMessage
				var clientInputMessage = Serializer.Deserialize<NetworkMessage<ClientInputMessage>>( data )!;
				var clientInput = clientInputMessage.Data;

				Log.Info( $@"BaseGameServer: Client {client} sent input message:
				LerpMsec: {clientInput.LerpMsec}
				Msec: {clientInput.Msec}
				ViewAngles: {clientInput.ViewAngles}
				ForwardMove: {clientInput.ForwardMove}
				SideMove: {clientInput.SideMove}
				UpMove: {clientInput.UpMove}
				Buttons: {clientInput.Buttons}" );
				break;

			default:
				Log.Error( $"BaseGameServer: Unknown message type {message.NetworkMessageType}" );
				break;
		}
	}
}
