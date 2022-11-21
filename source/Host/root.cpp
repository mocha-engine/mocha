#include "root.h"

#include <defs.h>
#include <globalvars.h>
#include <logmanager.h>
#include <managed/hostmanager.h>
#include <renderdocmanager.h>
#include <vulkan/rendermanager.h>

VmaAllocator* g_allocator; // Ugly duckling

RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;

void Root::StartUp()
{
	// TODO: How do we start up g_allocator like this?
	//		 Should we have a wrapper around VmaAllocator?
	//		 Should it be part of a 'RenderSystem'?

	g_logManager = new LogManager();
	g_logManager->StartUp();

	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->StartUp();

	g_renderManager = new RenderManager();
	g_renderManager->StartUp();

	g_hostManager = new HostManager( MANAGED_PATH, MANAGED_CLASS );
	g_hostManager->StartUp();
}

void Root::ShutDown()
{
	g_hostManager->ShutDown();
	g_renderManager->ShutDown();
	g_renderdocManager->ShutDown();
	g_logManager->ShutDown();
}

void Root::Run()
{
	g_renderManager->Run();
}