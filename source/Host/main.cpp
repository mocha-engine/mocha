#include <root.h>
#undef main

int main()
{
	auto& root = Root::GetInstance();

	root.StartUp();
	root.Run();
	root.ShutDown();

	return 0;
}