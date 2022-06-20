using System.Net;
using System.Net.Sockets;

namespace Mocha.Common;

public class RemoteConsoleServer : RemoteConsoleConnection
{
	private TcpListener tcpListener;
	private TcpClient tcpClient;

	public RemoteConsoleServer()
	{
		tcpListener = new TcpListener( IPAddress.Loopback, 2794 );
		tcpListener.Start();

		var thread = new Thread( ListenThread );
		thread.Start();
	}

	protected override void ListenThread()
	{
		while ( true )
		{
			byte[] buf = new byte[4096];

			tcpClient = tcpListener.AcceptTcpClient();
			stream = tcpClient.GetStream();

			ConnectionStarted();

			while ( (_ = stream.Read( buf, 0, buf.Length )) > 0 )
			{
				MessageReceived( buf );
			}
		}
	}

	public override void ConnectionStarted()
	{
		Log.Trace( "Connected to remote console instance" );
	}
}
