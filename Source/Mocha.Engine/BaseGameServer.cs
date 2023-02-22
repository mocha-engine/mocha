using Mocha.Networking;

namespace Mocha;
public class BaseGameServer : Server
{
	public BaseGameServer()
	{
		RegisterHandler<ClientInputMessage>( OnClientInputMessage );
	}

	public override void OnClientConnected( IConnection connection )
	{
		if ( connection is not ClientConnection client )
			return;

		Log.Info( $"BaseGameServer: Client {client} connected" );

		// Send initial HandshakeMessage
		var handshakeMessage = new HandshakeMessage();
		handshakeMessage.TickRate = Core.TickRate;
		handshakeMessage.Nickname = client.Nickname;
		client.Send( handshakeMessage );

		// Send initial SnapshotUpdateMessage
		var snapshotUpdateMessage = new SnapshotUpdateMessage();
		snapshotUpdateMessage.PreviousTimestamp = 0;
		snapshotUpdateMessage.CurrentTimestamp = 0;
		snapshotUpdateMessage.SequenceNumber = 0;
		snapshotUpdateMessage.EntityChanges.Add( new SnapshotUpdateMessage.EntityChange( 0, new List<SnapshotUpdateMessage.EntityFieldChange>() ) );
		client.Send( snapshotUpdateMessage );
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
