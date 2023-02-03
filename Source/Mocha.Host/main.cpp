#include <root.h>
#include <Windows.h>
#undef main

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	// Set thread name
	HRESULT hr = SetThreadDescription( GetCurrentThread(), L"Mocha Native Thread" );
	
	auto& root = Root::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}