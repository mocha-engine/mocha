using Mocha.Common;
using System.Text;

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

		// Let's send a packet every frame to the server...
		_nativeClient.SendData( Encoding.ASCII.GetBytes( "Boop\0" ).ToInterop() );
	}
}
