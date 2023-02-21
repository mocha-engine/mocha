using Mocha.Networking;

namespace Mocha;
public class BaseGameServer : Server
{
	public BaseGameServer()
	{
		RegisterHandler<ClientInputMessage>( OnClientInputMessage );
	}

	public override void OnClientConnected( IConnection client )
	{
		Log.Info( $"BaseGameServer: Client {client} connected" );
	}

	public override void OnClientDisconnected( IConnection client )
	{
		Log.Info( $"BaseGameServer: Client {client} disconnected" );
	}

	public override void OnMessageReceived( IConnection client, byte[] data )
	{
		InvokeHandler( client, data );
	}

	public void OnClientInputMessage( IConnection client, ClientInputMessage clientInputMessage )
	{
		Log.Info( $@"BaseGameServer: Client {client} sent input message:
			ViewAngles: {clientInputMessage.ViewAnglesP}, {clientInputMessage.ViewAnglesY}, {clientInputMessage.ViewAnglesR}
			Direction: {clientInputMessage.DirectionX}, {clientInputMessage.DirectionY}, {clientInputMessage.DirectionZ}
			Left: {clientInputMessage.Left}
			Right: {clientInputMessage.Right}
			Middle: {clientInputMessage.Middle}" );

		client.Disconnect( "Kick Test" );
	}
}
