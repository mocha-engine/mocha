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

			try
			{
				while ( tcpClient.Connected && (_ = stream.Read( buf, 0, buf.Length )) > 0 )
				{
					var obj = Serializer.Deserialize<ConsolePacket<ConsoleMessage>>( buf );
					OnLog?.Invoke( obj.Data );
				}
			}
			catch { }
		}
	}

	public override void MessageReceived( byte[] data )
	{
		var obj = Serializer.Deserialize<ConsolePacket<ConsoleMessage>>( data );
		OnLog?.Invoke( obj.Data );
	}
}
