#include "renderdocmanager.h"

#include <cvarmanager.h>
#include <defs.h>
#include <renderdoc_app.h>
#include <spdlog/spdlog.h>
#include <globalvars.h>

void RenderdocManager::Startup()
{
	if ( !EngineProperties::Renderdoc )
		return;

	//
	// Renderdoc is not compatible with any of the raytracing
	// extensions, and will break everything if we try to attach.
	// https://github.com/baldurk/renderdoc/issues/2317
	//
	// If you want to attach renderdoc, disable raytracing.
	//
	if ( EngineProperties::Raytracing )
	{
		spdlog::info( "Renderdoc and raytracing are not compatible with each other - disabling renderdoc" );
		return;
	}

	RENDERDOC_API_1_5_0* rdoc_api = NULL;

	auto renderdocDll = LoadLibraryA( "renderdoc.dll" );
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

void RenderdocManager::Shutdown()
{
	if ( EngineProperties::Raytracing )
		return;

	if ( !EngineProperties::Renderdoc )
		return;
}