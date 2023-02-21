namespace Mocha.Networking;

public partial class Client
{
	public readonly struct ServerConnection : IConnection
	{
		public void Disconnect( string reason )
		{
			throw new NotImplementedException();
		}

		public void Send<T>( T message ) where T : IBaseNetworkMessage, new()
		{
			throw new NotImplementedException();
		}

		public void SendData( byte[] data )
		{
			throw new NotImplementedException();
		}
	}
}
