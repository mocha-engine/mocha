using VConsoleLib;

var server = new VConsoleServer();

server.OnCommand += ( command ) =>
{
	Console.WriteLine( $"Received command: {command}" );
	server.Log( $"Unknown command '{command.Replace( "\0", "" )}'", 0xFF0000FF );
};

Console.ReadLine();
