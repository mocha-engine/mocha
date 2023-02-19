using Mocha.Common;
using System.Runtime.InteropServices;

namespace Mocha.Networking;

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
		private string RemoteAddress { get; init; }

		public ConnectedClient( uint nativeHandle )
		{
			NativeHandle = nativeHandle;
			RemoteAddress = GetAddress();
		}

		private string GetAddress()
		{
			return Instance._nativeServer.GetRemoteAddress( NativeHandle );
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

		public void Send<T>( T message ) where T : BaseNetworkMessage, new()
		{
			var wrapper = new NetworkMessageWrapper<T>();
			wrapper.Data = message;
			wrapper.NetworkMessageType = 0;

			var bytes = wrapper.Serialize();
			SendData( bytes );
		}

		public override string ToString()
		{
			return RemoteAddress;
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

				_connectedClients.Add( client );
				OnClientConnected( client );
			}
		) );

		_nativeServer.SetClientDisconnectedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr clientHandlePtr ) =>
			{
				var client = ConnectedClient.FromPointer( clientHandlePtr );

				_connectedClients.Remove( client );
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

	public virtual void OnClientConnected( ConnectedClient client )
	{
	}

	public virtual void OnClientDisconnected( ConnectedClient client )
	{
	}

	public virtual void OnMessageReceived( ConnectedClient client, byte[] data )
	{
	}
}
