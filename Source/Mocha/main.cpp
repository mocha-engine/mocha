#include <Windows.h>
#include <clientroot.h>
#include <globalvars.h>
#include <iostream>
#include <serverroot.h>
#include <thread>

void ClientThread( Root& root )
{
	root.Run();
}

void ListenServerThread( Root& root )
{
	root.Run();
}

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	auto& clientRoot = ClientRoot::GetInstance();
	clientRoot.Startup();

	clientRoot.Run();
	clientRoot.Shutdown();

	return 0;
}
