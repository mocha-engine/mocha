#include "root.h"

#include <defs.h>
#include <globalvars.h>
#include <logmanager.h>
#include <managed/hostmanager.h>
#include <renderdocmanager.h>
#include <vulkan/rendermanager.h>
#include <edict.h>

//
// These global variables are all defined in globalvars.h, 
// because the naming makes more sense (imagine if we 
// included Root.h everywhere!)
//

VmaAllocator* g_allocator; // Ugly duckling

RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;
EntityManager* g_entityDictionary;

void Root::Startup()
{
	// TODO: How do we start up g_allocator like this?
	//		 Should we have a wrapper around VmaAllocator?
	//		 Should it be part of a 'RenderSystem'?

	g_logManager = new LogManager();
	g_logManager->Startup();

	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->Startup();

	g_entityDictionary = new EntityManager();
	g_entityDictionary->Startup();

	g_renderManager = new RenderManager();
	g_renderManager->Startup();

	g_hostManager = new HostManager( MANAGED_PATH, MANAGED_CLASS );
	g_hostManager->Startup();
}

void Root::Shutdown()
{
	g_hostManager->Shutdown();
	g_renderManager->Shutdown();
	g_entityDictionary->Shutdown();
	g_renderdocManager->Shutdown();
	g_logManager->Shutdown();
}

void Root::Run()
{
	g_renderManager->Run();
}