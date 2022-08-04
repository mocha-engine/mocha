#include "CNativeWindow.h"

#if WINDOWS
#include <Windows.h>

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
#else
int main(int argc, char* argv[])
#endif
{
	CNativeWindow window;
	window.Create("Hello World", 1280, 720);
	window.Run();
	return 0;
}