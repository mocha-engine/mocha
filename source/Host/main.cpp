#include "thirdparty/renderdoc_app.h"
#include "vulkan/vk_engine.h"

#include "managed/ManagedHost.h"

#include <SDL2/SDL.h>
#include <iostream>
#include <spdlog/spdlog.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#undef main

void InitRenderdoc()
{
	RENDERDOC_API_1_5_0* rdoc_api = NULL;

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

	// Setup spdlog
	auto managed = spdlog::stdout_color_mt( "managed" );
	auto main = spdlog::stderr_color_mt( "main" );
	auto renderer = spdlog::stderr_color_mt( "renderer" );
	spdlog::set_default_logger( main );
	spdlog::set_level( spdlog::level::trace );

	// Set pattern "time logger,8 type,8 message"
	spdlog::set_pattern( "%H:%M:%S %-8n %^%-8l%$ %v" );

	// Get current working directory
	char cwd[1024];
	_getcwd( cwd, sizeof( cwd ) );
	spdlog::info( "Current working directory: {}", cwd );

	// Start managed bullshit
	auto managedHost = ManagedHost( L".\\build\\Engine", L"Mocha.Main, Engine", L"Run" );

	CNativeEngine engine;

	engine.Init();
	engine.Run();
	engine.Cleanup();

	return 0;
}