#include <Misc/globalvars.h>
#include <Root/root.h>
#include <Root/serverroot.h>
#include <iostream>

int main()
{
	Globals::m_isDedicatedServer = true;
	
	ServerRoot root = ServerRoot();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}