#include "renderdoc.h"

#include "thirdparty/renderdoc_app.h"

#include <spdlog/spdlog.h>

void Renderdoc::StartUp()
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

void Renderdoc::ShutDown() {}