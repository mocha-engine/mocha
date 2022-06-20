using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mocha.Engine;

public class RemoteConsoleServer
{
	private TcpListener tcpListener;
	private TcpClient tcpClient;
	private NetworkStream stream;

	public RemoteConsoleServer()
	{
		tcpListener = new TcpListener( IPAddress.Loopback, 2794 );
		tcpListener.Start();

		var thread = new Thread( ListenThread );
		thread.Start();
	}

	private void SerializeAndSend<T>( T obj ) where T : struct
	{
		if ( stream != null && !stream.CanWrite )
			return;

		var consolePacket = new ConsolePacket<T>
		{
			ProtocolVersion = 1,
			Data = obj
		};

		var data = Serializer.Serialize( consolePacket );

		stream?.Write( data, 0, data.Length );
	}

	public void Write( Logger.Level level, string str, StackTrace stackTrace )
	{
		uint color = 0xFFFFFFFF;
		switch ( level )
		{
			case Logger.Level.Trace:
				color = 0xFFAAAAAA;
				break;
			case Logger.Level.Info:
				color = 0xFFFFFFFF;
				break;
			case Logger.Level.Warning:
				color = 0xAAAAAAFF;
				break;
			case Logger.Level.Error:
				color = 0xFF0000FF;
				break;
		}

		// TODO: make this not shit
		var obj = new ConsoleMessage()
		{
			Color = color,
			Message = str,
			CallingClass = stackTrace.GetFrame( 2 ).GetMethod().DeclaringType.Name,
			StackTrace = stackTrace.GetFrames().Select( x => x.ToString() ).ToArray()
		};

		SerializeAndSend( obj );
	}

	private void ListenThread()
	{
		while ( true )
		{
			byte[] buf = new byte[4096];

			tcpClient = tcpListener.AcceptTcpClient();
			stream = tcpClient.GetStream();

			Log.Trace( "Connected to remote console instance" );

			while ( (_ = stream.Read( buf, 0, buf.Length )) > 0 )
			{
				var bufStr = Encoding.ASCII.GetString( buf );
				var identifier = bufStr[..4];
			}
		}
	}
}
