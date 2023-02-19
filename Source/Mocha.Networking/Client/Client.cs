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
			ViewAnglesP = Input.Rotation.ToEulerAngles().X,
			ViewAnglesY = Input.Rotation.ToEulerAngles().Y,
			ViewAnglesR = Input.Rotation.ToEulerAngles().Z,

			DirectionX = Input.Direction.X,
			DirectionY = Input.Direction.Y,
			DirectionZ = Input.Direction.Z,

			Left = Input.Left,
			Right = Input.Right,
			Middle = Input.Middle
		};

		Send( clientInput );
	}

	public void Send<T>( T message ) where T : BaseNetworkMessage, new()
	{
		var wrapper = new NetworkMessageWrapper<T>();
		wrapper.Data = message;
		wrapper.NetworkMessageType = 0;

		var bytes = wrapper.Serialize();
		_nativeClient.SendData( bytes.ToInterop() );
	}
}
