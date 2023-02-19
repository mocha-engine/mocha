using Mocha.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Mocha.Networking;

[StructLayout( LayoutKind.Sequential )]
struct ValveSocketReceivedMessage
{
	public IntPtr connectionHandle;
	public int size;
	public IntPtr data;
};

public class Server
{
	private static Server Instance { get; set; }

	private Glue.ValveSocketServer _nativeServer;

	// I don't like the idea of managing two separate lists (one native,
	// one managed) for this, but I think it might be unavoidable. :(
	private List<ConnectedClient> _connectedClients = new();

	/// <summary>
	/// Represents a client that a server has a connection to
	/// </summary>
	public readonly struct ConnectedClient
	{
		public uint NativeHandle { get; init; }

		public ConnectedClient( uint nativeHandle )
		{
			NativeHandle = nativeHandle;
		}

		public static ConnectedClient FromPointer( IntPtr pointer )
		{
			var clientHandle = (uint)pointer;
			return new ConnectedClient( clientHandle );
		}

		public void SendData( byte[] data )
		{
			Instance._nativeServer.SendData( NativeHandle, data.ToInterop() );
		}
	}

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
				var client = ConnectedClient.FromPointer( clientHandlePtr );
				OnClientConnected( client );
			}
		) );

		_nativeServer.SetClientDisconnectedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr clientHandlePtr ) =>
			{
				var client = ConnectedClient.FromPointer( clientHandlePtr );
				OnClientDisconnected( client );
			}
		) );

		_nativeServer.SetDataReceivedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr receivedMessagePtr ) =>
			{
				var receivedMessage = Marshal.PtrToStructure<ValveSocketReceivedMessage>( receivedMessagePtr );
				var client = ConnectedClient.FromPointer( receivedMessage.connectionHandle );
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

	public void OnClientConnected( ConnectedClient client )
	{
		Log.Info( "Managed: Client was connected" );

		_connectedClients.Add( client );

		// Handshake or something
		client.SendData( new byte[] { 0x20, 0x20, 0x20, 0x20 } );
	}

	public void OnClientDisconnected( ConnectedClient client )
	{
		Log.Info( "Managed: Client was disconnected" );

		// Etc...
		_connectedClients.Remove( client );
	}

	public void OnMessageReceived( ConnectedClient client, byte[] data )
	{
		Log.Info( "Managed: Received a message" );
		Log.Info( $"Managed: {Encoding.ASCII.GetString( data )}" );

		// Etc...
	}
}
