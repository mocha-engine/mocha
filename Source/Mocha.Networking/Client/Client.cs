using Mocha.Common;

namespace Mocha.Networking;

public class Client
{
	private Glue.ValveSocketClient _nativeClient;

	public Client( string ipAddress, ushort port = 10570 )
	{
		_nativeClient = new Glue.ValveSocketClient( ipAddress, port );
	}

	[Event.Tick]
	public void OnTick()
	{
		_nativeClient.PumpEvents();
		_nativeClient.RunCallbacks();
	}
}
