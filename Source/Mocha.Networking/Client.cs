using Steamworks;
using Steamworks.Data;

namespace Mocha.Networking.Client;

public class ClientConnection : ConnectionManager
{
	public override void OnConnecting( ConnectionInfo info )
	{
		Console.WriteLine( $"Connecting to {info.Address}" );
	}

	public override void OnConnected( ConnectionInfo info )
	{
		Console.WriteLine( $"Connected to {info.Address}" );
	}

	public override void OnDisconnected( ConnectionInfo info )
	{
		Console.WriteLine( $"Disconnected from {info.Address}" );
	}

	public override void OnMessage( nint data, int size, long messageNum, long recvTime, int channel )
	{
		Console.WriteLine( $"We got a message on channel {channel}!" );

		// Send data back
		Connection.SendMessage( data, size, SendType.Reliable );
	}
}
