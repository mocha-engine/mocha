#include <globalvars.h>
#include <iostream>
#include <root.h>
#include <serverroot.h>

int main()
{
	g_executingRealm = REALM_SERVER;

	auto& root = ServerRoot::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}