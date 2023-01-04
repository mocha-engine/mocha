#define _CRTDBG_MAP_ALLOC

#include "root.h"

#include <crtdbg.h>
#include <defs.h>
#include <edict.h>
#include <gamesettings.h>
#include <globalvars.h>
#include <hostmanager.h>
#include <inputmanager.h>
#include <logmanager.h>
#include <physicsmanager.h>
#include <renderdocmanager.h>
#include <rendermanager.h>
#include <stdlib.h>

//
// These global variables are all defined in globalvars.h,
// because the naming makes more sense (imagine if we
// included Root.h everywhere!)
//
RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;
EntityManager* g_entityDictionary;
PhysicsManager* g_physicsManager;
InputManager* g_inputManager;
BaseRenderContext* g_renderContext; // TODO??

float g_curTime;
float g_frameTime;
Vector3 g_cameraPos;
Quaternion g_cameraRot;
float g_cameraFov;
float g_cameraZNear;
float g_cameraZFar;
RenderDebugViews g_debugView;

void Root::Startup()
{
	g_logManager = new LogManager();
	g_logManager->Startup();

	// HACK: CvarManager needs to start up before *everything* excluding logger
	CVarManager::Instance().Startup();

#ifdef _RENDERDOC
	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->Startup();
#endif

	g_entityDictionary = new EntityManager();
	g_entityDictionary->Startup();

	// HACK: This goes BEFORE the ctor because we need it before
	// fields on PhysicsManager get assigned to..
	PhysicsManager::PreInit();
	g_physicsManager = new PhysicsManager();
	g_physicsManager->Startup();

	g_inputManager = new InputManager();
	g_inputManager->Startup();

	g_renderManager = new RenderManager();
	g_renderManager->Startup();

	g_hostManager = new HostManager( MANAGED_PATH, MANAGED_CLASS );
	g_hostManager->Startup();
}

void Root::Shutdown()
{
	g_hostManager->Shutdown();
	g_renderManager->Shutdown();
	g_inputManager->Shutdown();
	g_physicsManager->Shutdown();
	g_entityDictionary->Shutdown();

#if _RENDERDOC
	g_renderdocManager->Shutdown();
#endif

	// HACK: CvarManager needs to shut down after *everything* excluding logger
	CVarManager::Instance().Shutdown();

	g_logManager->Shutdown();

	_CrtDumpMemoryLeaks();
}

void Root::Run()
{
	g_renderManager->Run();
}