#include <globalvars.h>
#include <iostream>
#include <root.h>

int main()
{
	g_executingRealm = REALM_CLIENT;

	auto& root = Root::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}