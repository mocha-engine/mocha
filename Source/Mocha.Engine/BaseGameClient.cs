using Mocha.Networking;

namespace Mocha;
public class BaseGameClient : Client
{
	private ServerConnection _connection;

	public BaseGameClient( string ipAddress, ushort port = 10570 ) : base( ipAddress, port )
	{
		_connection = new ServerConnection();
		RegisterHandler<KickedMessage>( OnKickedMessage );
	}

	public override void OnMessageReceived( byte[] data )
	{
		InvokeHandler( _connection, data );
	}

	public void OnKickedMessage( IConnection connection, KickedMessage kickedMessage )
	{
		Log.Info( $"BaseGameClient: We were kicked: '{kickedMessage.Reason}'" );
	}
}
