using Mocha.Common;
using System.Runtime.InteropServices;

namespace Mocha.Networking;

public partial class Client : ConnectionManager
{
	private Glue.ValveSocketClient _nativeClient;

	public Client( string ipAddress, ushort port = 10570 )
	{
		_nativeClient = new Glue.ValveSocketClient( ipAddress, port );
		RegisterNativeCallbacks();
	}

	private void RegisterNativeCallbacks()
	{
		_nativeClient.SetDataReceivedCallback(
			CallbackDispatcher.RegisterCallback( ( IntPtr receivedMessagePtr ) =>
			{
				var receivedMessage = Marshal.PtrToStructure<ValveSocketReceivedMessage>( receivedMessagePtr );
				var data = new byte[receivedMessage.size];
				Marshal.Copy( receivedMessage.data, data, 0, receivedMessage.size );

				OnMessageReceived( data );
			}
		) );
	}

	public virtual void OnMessageReceived( byte[] data )
	{
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

	public void Send<T>( T message ) where T : IBaseNetworkMessage, new()
	{
		var wrapper = new NetworkMessageWrapper();
		wrapper.Data = NetworkSerializer.Serialize( message );
		wrapper.Type = message.MessageID;

		var bytes = NetworkSerializer.Serialize( wrapper );
		_nativeClient.SendData( bytes.ToInterop() );
	}
}
