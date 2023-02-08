#include <globalvars.h>
#include <iostream>
#include <root.h>
#include <serverroot.h>

int main()
{
	auto& root = ServerRoot::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}