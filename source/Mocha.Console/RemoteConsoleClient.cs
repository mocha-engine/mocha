using Mocha.Common;
using System.Net;
using System.Net.Sockets;

namespace Mocha.Console;

public class RemoteConsoleClient
{
	private TcpClient tcpClient;
	private NetworkStream stream;

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

	private void ListenThread()
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
}
