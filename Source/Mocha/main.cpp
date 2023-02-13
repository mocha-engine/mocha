#include <Misc/globalvars.h>
#include <Root/clientroot.h>
#include <Root/serverroot.h>
#include <Windows.h>
#include <iostream>
#include <thread>

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	ClientRoot clientRoot = ClientRoot();
	clientRoot.Startup();

	clientRoot.Run();
	clientRoot.Shutdown();

	return 0;
}
