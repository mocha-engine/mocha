using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mocha.Common;

public class RemoteConsoleConnection
{
	protected NetworkStream stream;

	protected void SerializeAndSend<T>( T obj ) where T : struct
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

		SerializeAndSend( obj );
	}

	protected virtual void ListenThread()
	{
	}

	public virtual void ConnectionStarted()
	{

	}

	public virtual void MessageReceived( byte[] data )
	{
	}
}
