using System.Text.Json;

namespace Mocha.Networking;

public class ConnectionManager
{
	protected readonly record struct MessageHandler( Type type, Action<IConnection, object> Action );
	protected Dictionary<string, MessageHandler> _messageHandlers = new();

	protected void RegisterHandler<T>( Action<IConnection, T> handler ) where T : IBaseNetworkMessage
	{
		var messageId = typeof( T ).FullName!;
		var messageHandler = new MessageHandler( typeof( T ), ( connection, data ) => handler?.Invoke( connection, (T)data ) );
		_messageHandlers.Add( messageId, messageHandler );
	}

	protected void InvokeHandler( IConnection connection, byte[] data )
	{
		var message = JsonSerializer.Deserialize<NetworkMessageWrapper>( data )!;

		foreach ( var (type, handler) in _messageHandlers )
		{
			if ( type == message.Type )
			{
				var messageData = JsonSerializer.Deserialize( message.Data, handler.type )!;
				handler.Action?.Invoke( connection, messageData );
				return;
			}
		}

		Log.Error( $"ConnectionManager: Unknown message type '{message.Type}'" );
	}
}
