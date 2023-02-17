using Mocha.Common;

namespace Mocha.Networking;

public class Server
{
	private Glue.ValveSocketServer _nativeServer;

	// I don't like the idea of managing two separate lists (one native,
	// one managed) for this, but I think it might be unavoidable. :(
	private List<ConnectedClient> _connectedClients;

	/// <summary>
	/// Represents a client that a server has a connection to
	/// </summary>
	public struct ConnectedClient
	{
		public uint NativeHandle { get; init; }
		private Server Server { get; init; }

		public ConnectedClient( Server server, uint nativeHandle )
		{
			Server = server;
			NativeHandle = nativeHandle;
		}

		public void SendData( byte[] data )
		{
			Server._nativeServer.SendData( NativeHandle, data.ToInterop() );
		}
	}

	public Server( ushort port = 10570 )
	{
		_nativeServer = new Glue.ValveSocketServer( port );
		_nativeServer.SetConnectedCallback( CallbackDispatcher.RegisterCallback( Test ) );
	}

	[Event.Tick]
	public void OnTick()
	{
		_nativeServer.PumpEvents();
	}

	public void Test()
	{
		Log.Trace( "!!!! Test !!!!" );
	}

	public void OnClientConnected( ConnectedClient client )
	{
		// TODO
		_connectedClients.Add( client );

		// Handshake or something
		client.SendData( new byte[] { 0x20, 0x20, 0x20, 0x20 } );
	}

	public void OnClientDisconnected( ConnectedClient client )
	{
		// Etc...
		_connectedClients.Remove( client );
	}

	public void OnMessageReceived( ConnectedClient client, byte[] data )
	{
		// Etc...

		// Echo
		client.SendData( data );
	}
}
