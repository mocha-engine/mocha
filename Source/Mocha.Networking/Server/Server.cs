using Mocha.Common;
using System.Runtime.InteropServices;

namespace Mocha.Networking;

public partial class Server : ConnectionManager
{
	private static Server Instance { get; set; }

	private Glue.ValveSocketServer _nativeServer;

	// I don't like the idea of managing two separate lists (one native,
	// one managed) for this, but I think it might be unavoidable. :(
	private List<IConnection> _connectedClients = new();

	public Server( ushort port = 10570 )
	{
		Instance = this;

		_nativeServer = new Glue.ValveSocketServer( port );
		RegisterNativeCallbacks();
	}

	private void RegisterNativeCallbacks()
	{
		_nativeServer.SetClientConnectedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr clientHandlePtr ) =>
			{
				var client = ClientConnection.CreateFromPointer( clientHandlePtr );

				_connectedClients.Add( client );
				OnClientConnected( client );
			}
		) );

		_nativeServer.SetClientDisconnectedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr clientHandlePtr ) =>
			{
				var client = ClientConnection.CreateFromPointer( clientHandlePtr );

				_connectedClients.Remove( client );
				OnClientDisconnected( client );
			}
		) );

		_nativeServer.SetDataReceivedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr receivedMessagePtr ) =>
			{
				var receivedMessage = Marshal.PtrToStructure<ValveSocketReceivedMessage>( receivedMessagePtr );
				var client = ClientConnection.CreateFromPointer( receivedMessage.connectionHandle );
				var data = new byte[receivedMessage.size];
				Marshal.Copy( receivedMessage.data, data, 0, receivedMessage.size );

				OnMessageReceived( client, data );
			}
		) );
	}

	public void Update()
	{
		_nativeServer.PumpEvents();
		_nativeServer.RunCallbacks();
	}

	public virtual void OnClientConnected( IConnection client )
	{
	}

	public virtual void OnClientDisconnected( IConnection client )
	{
	}

	public virtual void OnMessageReceived( IConnection client, byte[] data )
	{
	}
}
