#include <Misc/globalvars.h>
#include <Root/clientroot.h>
#include <Root/serverroot.h>
#include <Windows.h>
#include <iostream>
#include <thread>
#include <processenv.h>
#include <shellapi.h>

/// <summary>
/// Gets a command line argument option.
/// </summary>
char* getCmdOption( char** begin, char** end, const std::string& option );

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, LPSTR cmdline, int cmdshow )
{
	SetProcessDPIAware();

	Globals::m_activeProjectPath = getCmdOption( __argv, __argv + __argc, "-project" );
	Globals::m_isDedicatedServer = false;

	ClientRoot root = ClientRoot();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}

// From https://stackoverflow.com/a/868894
char* getCmdOption( char** begin, char** end, const std::string& option )
{
	char** itr = std::find( begin, end, option );
	if ( itr != end && ++itr != end )
	{
		return *itr;
	}
	return 0;
}
