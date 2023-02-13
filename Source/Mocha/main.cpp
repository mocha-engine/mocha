#include <Misc/globalvars.h>
#include <Root/clientroot.h>
#include <Root/serverroot.h>
#include <Windows.h>
#include <iostream>
#include <thread>

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	Globals::m_isDedicatedServer = false;

	ClientRoot root = ClientRoot();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}
