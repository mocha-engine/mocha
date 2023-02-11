#include <globalvars.h>
#include <iostream>
#include <root.h>
#include <serverroot.h>

int main()
{
	ServerRoot root = ServerRoot();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}