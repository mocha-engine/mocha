namespace Mocha.Networking;

/// <summary>
/// Represents a connection between a client and a server
/// </summary>
public interface IConnection
{
	void Disconnect( string reason );
}
