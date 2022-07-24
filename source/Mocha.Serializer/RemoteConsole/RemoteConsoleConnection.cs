using Mocha.Common.Serialization;
using System.Net.Sockets;

namespace Mocha.Common;

public class RemoteConsoleConnection
{
	protected DateTime lastServerKeepAlive = DateTime.Now; // Last received a keepalive
	protected DateTime lastClientKeepAlive = DateTime.Now; // Last sent a keepalive

	protected NetworkStream stream;

	protected void SerializeAndSend<T>( string identifier, T obj ) where T : struct
	{
		if ( stream != null && !stream.CanWrite )
			return;

		var consolePacket = new ConsolePacket
		{
			ProtocolVersion = 1,
			Identifier = identifier,
			Data = Serializer.Serialize( obj )
		};

		var data = Serializer.Serialize( consolePacket );

		stream?.Write( data, 0, data.Length );
	}

	public void Write( Logger.Level level, string str, string callingClass, string[] stackTrace )
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
			CallingClass = callingClass,
			StackTrace = stackTrace
		};

		SerializeAndSend( "PRNT", obj );
	}

	protected virtual void ListenThread()
	{
	}

	public virtual void ConnectionStarted()
	{
		lastClientKeepAlive = DateTime.Now;
		lastServerKeepAlive = DateTime.Now;
	}

	public virtual void MessageReceived( byte[] data )
	{
	}
}
