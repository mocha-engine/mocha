#include "thirdparty/renderdoc_app.h"
#include "vulkan/vk_engine.h"

#include <SDL2/SDL.h>
#include <iostream>
#include <spdlog/spdlog.h>
#undef main

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
	assert( ret == 1 );
}

int main()
{
	InitRenderdoc();

	spdlog::set_level( spdlog::level::trace );

	// Set pattern to [time] [type, pad right] [message]
	spdlog::set_pattern( "[%H:%M:%S] %^%-8l%$ %v" );

	CNativeEngine engine;

	engine.Init();
	engine.Run();
	engine.Cleanup();

	return 0;
}