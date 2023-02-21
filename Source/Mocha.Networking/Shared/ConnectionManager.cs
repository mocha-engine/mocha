using System.Text.Json;

namespace Mocha.Networking;

public class ConnectionManager
{
	protected readonly record struct MessageHandler( Type type, Action<IConnection, object> Action );
	protected Dictionary<int, MessageHandler> _messageHandlers = new();

	protected void RegisterHandler<T>( Action<IConnection, T> handler ) where T : IBaseNetworkMessage
	{
		var messageId = (int)typeof( T ).GetProperty( "MessageId" )!.GetValue( null, null )!;
		_messageHandlers.Add( messageId, new MessageHandler( typeof( T ), ( connection, data ) => handler?.Invoke( connection, (T)data ) ) );
	}

	protected void InvokeHandler( IConnection connection, byte[] data )
	{
		var message = JsonSerializer.Deserialize<NetworkMessageWrapper>( data )!;

		foreach ( var (messageId, handler) in _messageHandlers )
		{
			if ( messageId == message.NetworkMessageType )
			{
				var messageData = JsonSerializer.Deserialize( message.Data, handler.type )!;
				handler.Action?.Invoke( connection, messageData );
				return;
			}
		}

		Log.Error( $"ConnectionManager: Unknown message type '{message.NetworkMessageType}'" );
	}
}
