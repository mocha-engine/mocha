#include <Misc/globalvars.h>
#include <Root/root.h>
#include <Root/serverroot.h>
#include <iostream>

/// <summary>
/// Gets a command line argument option.
/// </summary>
char* getCmdOption( char** begin, char** end, const std::string& option );

int main( int argc, char* argv[] )
{
	Globals::m_activeProjectPath = getCmdOption( argv, argv + argc, "-project" );
	Globals::m_isDedicatedServer = true;
	
	ServerRoot root = ServerRoot();

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