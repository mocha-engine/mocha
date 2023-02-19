using Mocha.Networking;
using System.Text;

namespace Mocha;
public class BaseGameServer : Server
{
	public override void OnClientConnected( ConnectedClient client )
	{
		Log.Trace( $"BaseGameServer: Client {client} connected" );
	}

	public override void OnClientDisconnected( ConnectedClient client )
	{
		Log.Trace( $"BaseGameServer: Client {client} disconnected" );
	}

	public override void OnMessageReceived( ConnectedClient client, byte[] data )
	{
		Log.Trace( $"BaseGameServer: Received message '{Encoding.ASCII.GetString( data )}' from client {client}" );
	}
}
