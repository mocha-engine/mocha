namespace Mocha.Networking;

public class Client
{
	public Client( string ipAddress, ushort port = 10570 )
	{
		var valveSocketClient = new Glue.ValveSocketClient( ipAddress, port );
	}
}
