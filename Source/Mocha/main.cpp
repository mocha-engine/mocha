#include <iostream>
#include <root.h>

int main()
{
	auto& root = Root::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}