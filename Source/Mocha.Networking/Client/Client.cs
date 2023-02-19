using Mocha.Common;

namespace Mocha.Networking;

public class Client
{
	private Glue.ValveSocketClient _nativeClient;

	public Client( string ipAddress, ushort port = 10570 )
	{
		_nativeClient = new Glue.ValveSocketClient( ipAddress, port );
	}

	public void Update()
	{
		_nativeClient.PumpEvents();
		_nativeClient.RunCallbacks();

		var clientInput = new ClientInputMessage()
		{
			Buttons = 0,
			ForwardMove = Input.Direction.X,
			SideMove = Input.Direction.Y,
			UpMove = Input.Direction.Z,
			LerpMsec = 100,
			Msec = 100,
			ViewAngles = Input.Rotation.ToEulerAngles()
		};

		Send( clientInput );
	}

	public void Send<T>( T message ) where T : BaseNetworkMessage, new()
	{
		var networkMessage = new NetworkMessage<T>();
		networkMessage.Data = message;
		networkMessage.NetworkMessageType = 0;

		var bytes = Serializer.Serialize( networkMessage );
		_nativeClient.SendData( bytes.ToInterop() );
	}
}
