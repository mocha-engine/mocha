namespace Mocha.Networking;

public class ConnectionManager
{
	protected readonly record struct MessageHandler(Type type, Action<IConnection, object> Action);
	private Dictionary<MessageID, MessageHandler> _messageHandlers = new();

	protected void RegisterHandler<T>( Action<IConnection, T> handler ) where T : IBaseNetworkMessage
	{
		var instance = Activator.CreateInstance<T>() as IBaseNetworkMessage;
		var messageId = instance.MessageID;

		var messageHandler = new MessageHandler( typeof( T ), ( connection, data ) => handler?.Invoke( connection, (T)data ) );
		_messageHandlers.Add( messageId, messageHandler );
	}

	protected void InvokeHandler( IConnection connection, byte[] data )
	{
		var message = NetworkSerializer.Deserialize<NetworkMessageWrapper>( data )!;

		foreach ( var (type, handler) in _messageHandlers )
		{
			if ( type == message.Type )
			{
				var messageData = NetworkSerializer.Deserialize( message.Data, handler.type )!;
				handler.Action?.Invoke( connection, messageData );
				return;
			}
		}

		Log.Error( $"ConnectionManager: Unknown message type '{message.Type}'" );
	}
}
