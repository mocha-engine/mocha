using Mocha.Common;

namespace Mocha.Networking;

public partial class Server
{
	public readonly struct ClientConnection : IConnection
	{
		public uint NativeHandle { get; init; }
		private string RemoteAddress { get; init; }

		private ClientConnection( uint nativeHandle )
		{
			NativeHandle = nativeHandle;
			RemoteAddress = GetAddress();
		}

		public static ClientConnection CreateFromPointer( IntPtr pointer )
		{
			var clientHandle = (uint)pointer;
			return new( clientHandle );
		}

		private string GetAddress()
		{
			return Instance._nativeServer.GetRemoteAddress( NativeHandle );
		}

		public void SendData( byte[] data )
		{
			Instance._nativeServer.SendData( NativeHandle, data.ToInterop() );
		}

		public void Send<T>( T message ) where T : IBaseNetworkMessage, new()
		{
			var wrapper = new NetworkMessageWrapper();
			wrapper.Data = NetworkSerializer.Serialize( message );
			wrapper.NetworkMessageType = (int)typeof( T ).GetProperty( "MessageId" )!.GetValue( null, null )!;

			var bytes = NetworkSerializer.Serialize( wrapper );
			SendData( bytes );
		}

		public void Disconnect( string? reason = null! )
		{
			var kickedMessage = new KickedMessage();
			if ( reason != null )
				kickedMessage.Reason = reason;

			Send( kickedMessage );
			Instance._nativeServer.Disconnect( NativeHandle );
		}

		public override string ToString()
		{
			return RemoteAddress;
		}
	}
}
