using Mocha.Common.Serialization;
using System.Net;
using System.Net.Sockets;

namespace Mocha.Common;

public class RemoteConsoleClient : RemoteConsoleConnection
{
	private TcpClient tcpClient;
	public Action<ConsoleMessage> OnLog;

	public RemoteConsoleClient()
	{
		tcpClient = new TcpClient();

		var thread = new Thread( ConnectThread );
		thread.Start();
	}

	private void ConnectThread()
	{
		while ( true )
		{
			try
			{
				if ( tcpClient.ConnectAsync( IPAddress.Loopback, 2794 ).Wait( 2500 ) )
				{
					stream = tcpClient.GetStream();
					var thread = new Thread( ListenThread );
					thread.Start();

					return;
				}
			}
			catch
			{
			}
		}
	}

	protected override void ListenThread()
	{
		while ( tcpClient.Connected )
		{
			byte[] buf = new byte[4096];

			if ( (DateTime.Now - lastClientKeepAlive).TotalSeconds > 5f )
			{
				// Send a keepalive
				SerializeAndSend<ConsoleKeepalive>( "KEEP", new() );
				OnLog?.Invoke( ConsoleMessage.CreateGeneric( "Pushing a keepalive from client" ) );
				lastClientKeepAlive = DateTime.Now;
			}

			if ( (DateTime.Now - lastServerKeepAlive).TotalSeconds > 10f )
			{
				// Timed out
				OnLog?.Invoke( ConsoleMessage.CreateGeneric( "Timed out" ) );
				return;
			}

			try
			{
				int readCount;
				while ( tcpClient.Connected && (readCount = stream.Read( buf, 0, buf.Length )) > 0 )
				{
					var obj = Serializer.Deserialize<ConsolePacket>( buf );

					if ( obj.Identifier == "PRNT" )
					{
						var data = Serializer.Deserialize<ConsoleMessage>( obj.Data );
						OnLog?.Invoke( data );
					}
					else if ( obj.Identifier == "KEEP" )
					{
						var data = Serializer.Deserialize<ConsoleKeepalive>( obj.Data );
						lastServerKeepAlive = DateTime.Now;
					}
					else
					{
						throw new Exception( $"Unknown identifier '{obj.Identifier}'" );
					}
				}
			}
			catch ( Exception ex )
			{
				Console.WriteLine( ex );
			}
		}
	}
}
