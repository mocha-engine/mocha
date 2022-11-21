#include "root.h"

#include <globalvars.h>
#include <logger.h>
#include <managed/managedhost.h>
#include <renderdoc.h>
#include <vulkan/engine.h>

VmaAllocator* g_allocator;
NativeEngine* g_engine;
Logger* g_logger;
ManagedHost* g_managedHost;
Renderdoc* g_renderdoc;

void Root::StartUp()
{
	// TODO: How do we start up g_allocator like this?
	//		 Should we have a wrapper around VmaAllocator?
	//		 Should it be part of a 'RenderSystem'?

	g_logger = new Logger();
	g_logger->StartUp();

	g_renderdoc = new Renderdoc();
	g_renderdoc->StartUp();

	g_engine = new NativeEngine();
	g_engine->StartUp();

	g_managedHost = new ManagedHost( L".\\build\\Engine", L"Mocha.Main, Engine" );
	g_managedHost->StartUp();
}

void Root::ShutDown()
{
	g_managedHost->ShutDown();
	g_engine->ShutDown();
	g_renderdoc->ShutDown();
	g_logger->ShutDown();
}