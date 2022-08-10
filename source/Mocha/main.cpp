#include "CEngine.h"
#include "Globals.h"
#include "renderdoc_app.h"
#include "spdlog/spdlog.h"

#include <SDL2/SDL.h>

CImgui* g_Imgui = nullptr;
CEngine* g_Engine = nullptr;
bool g_EngineIsRunning = true;

void InitRenderdoc()
{
	RENDERDOC_API_1_2_0* rdoc_api = NULL;

	auto renderdocDll = LoadLibrary( L"renderdoc.dll" );
	if ( renderdocDll == nullptr )
	{
		spdlog::error( "Failed to load RenderDoc DLL - Error: %#x", GetLastError() );
		return;
	}

	auto renderdocFunc = ( pRENDERDOC_GetAPI )GetProcAddress( renderdocDll, "RENDERDOC_GetAPI" );
	if ( renderdocFunc == nullptr )
	{
		spdlog::error( "Failed to find RENDERDOC_GetAPI() from RenderDoc DLL handle." );
		return;
	}

	int ret = renderdocFunc( eRENDERDOC_API_Version_1_2_0, ( void** )&rdoc_api );
	rdoc_api->MaskOverlayBits( eRENDERDOC_Overlay_None, 0 );
	rdoc_api->MaskOverlayBits( eRENDERDOC_Overlay_FrameRate, 0 );
	assert( ret == 1 );
}

#undef main
int main( int argc, char* argv[] )
{
	InitRenderdoc();

	spdlog::set_level( spdlog::level::trace );
	// Set pattern to [date] [type, pad right] [message]
	spdlog::set_pattern( "[%Y-%m-%d] %^%-8l%$ %v" );

	g_Engine = new CEngine();
	g_Engine->Run();
	g_Engine->~CEngine();

	return 0;
}