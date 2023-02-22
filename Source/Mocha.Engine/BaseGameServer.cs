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

		foreach ( var entity in EntityRegistry.Instance )
		{
			var entityChange = new SnapshotUpdateMessage.EntityChange();
			entityChange.NetworkId = entity.NetworkId;
			entityChange.FieldChanges = new List<SnapshotUpdateMessage.EntityFieldChange>();
			entityChange.TypeName = entity.GetType().FullName!;

			if ( entity.NetworkId.IsLocal() )
				continue; // Not networked, skip

			foreach ( var field in entity.GetType().GetFields() )
			{
				var fieldChange = new SnapshotUpdateMessage.EntityFieldChange();
				fieldChange.FieldName = field.Name;
				fieldChange.Value = field.GetValue( entity );
				entityChange.FieldChanges.Add( fieldChange );
			}

			snapshotUpdateMessage.EntityChanges.Add( entityChange );
		}

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
		return;

		Log.Info( $@"BaseGameServer: Client {client} sent input message:
			ViewAngles: {clientInputMessage.ViewAnglesP}, {clientInputMessage.ViewAnglesY}, {clientInputMessage.ViewAnglesR}
			Direction: {clientInputMessage.DirectionX}, {clientInputMessage.DirectionY}, {clientInputMessage.DirectionZ}
			Left: {clientInputMessage.Left}
			Right: {clientInputMessage.Right}
			Middle: {clientInputMessage.Middle}" );
	}
}
