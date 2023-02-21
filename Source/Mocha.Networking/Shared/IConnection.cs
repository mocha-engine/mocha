namespace Mocha.Networking;

/// <summary>
/// Represents a connection between a client and a server
/// </summary>
public interface IConnection
{
	void SendData( byte[] data );
	void Send<T>( T message ) where T : IBaseNetworkMessage, new();

	void Disconnect( string reason );
}
