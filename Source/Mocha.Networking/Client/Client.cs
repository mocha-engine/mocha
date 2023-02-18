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
	}
}
