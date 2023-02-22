using Mocha.Networking;

namespace Mocha;
public class BaseGameClient : Client
{
	private ServerConnection _connection;

	public BaseGameClient( string ipAddress, ushort port = 10570 ) : base( ipAddress, port )
	{
		_connection = new ServerConnection();
		RegisterHandler<KickedMessage>( OnKickedMessage );
		RegisterHandler<SnapshotUpdateMessage>( OnSnapshotUpdateMessage );
		RegisterHandler<HandshakeMessage>( OnHandshakeMessage );
	}

	public override void OnMessageReceived( byte[] data )
	{
		InvokeHandler( _connection, data );
	}

	public void OnKickedMessage( IConnection connection, KickedMessage kickedMessage )
	{
		Log.Info( $"BaseGameClient: We were kicked: '{kickedMessage.Reason}'" );
	}

	public void OnSnapshotUpdateMessage( IConnection connection, SnapshotUpdateMessage snapshotUpdateMessage )
	{
		foreach ( var entityChange in snapshotUpdateMessage.EntityChanges )
		{
			Log.Info( $"BaseGameClient: Entity {entityChange.EntityId} changed" );
			foreach ( var fieldChange in entityChange.FieldChanges )
			{
				Log.Info( $"BaseGameClient: Entity {entityChange.EntityId} field {fieldChange.FieldName} changed to {fieldChange.Value}" );
			}
		}
	}

	public void OnHandshakeMessage( IConnection connection, HandshakeMessage handshakeMessage )
	{
		Log.Info( $"BaseGameClient: Handshake received. Tick rate: {handshakeMessage.TickRate}, nickname: {handshakeMessage.Nickname}" );
	}
}
