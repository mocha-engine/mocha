#include <globalvars.h>
#include <iostream>
#include <clientroot.h>
#include <Windows.h>

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	g_executingRealm = REALM_CLIENT;

	auto& root = ClientRoot::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}