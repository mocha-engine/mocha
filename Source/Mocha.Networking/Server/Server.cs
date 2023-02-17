namespace Mocha.Networking;

public class Server
{
	public Server( ushort port = 10570 )
	{
		var valveSocketServer = new Glue.ValveSocketServer( "0.0.0.0", port );
	}
}
