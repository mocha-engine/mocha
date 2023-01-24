#define _CRTDBG_MAP_ALLOC

#include "root.h"

#include <crtdbg.h>
#include <defs.h>
#include <entitymanager.h>
#include <globalvars.h>
#include <hostmanager.h>
#include <inputmanager.h>
#include <logmanager.h>
#include <physicsmanager.h>
#include <projectmanager.h>
#include <renderdocmanager.h>
#include <rendermanager.h>
#include <stdlib.h>

// These global variables are all defined in globalvars.h,
// because the naming makes more sense (imagine if we
// included Root.h everywhere!)
RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;
EntityManager* g_entityDictionary;
PhysicsManager* g_physicsManager;
InputManager* g_inputManager;
BaseRenderContext* g_renderContext; // TODO: Remove
CVarManager* g_cvarManager;
ProjectManager* g_projectManager;

float g_curTime;
float g_frameTime;
float g_tickTime;
int g_curTick;
Vector3 g_cameraPos;
Quaternion g_cameraRot;
float g_cameraFov;
float g_cameraZNear;
float g_cameraZFar;
RenderDebugViews g_debugView;

namespace EngineProperties
{
	StringCVar LoadedProject(
	    "project.current", "Samples\\mocha-minimal\\project.json", CVarFlags::Archive, "Which project should we load?" );
	BoolCVar Raytracing( "render.raytracing", true, CVarFlags::Archive, "Enable raytracing" );
	BoolCVar Renderdoc( "render.renderdoc", false, CVarFlags::Archive, "Enable renderdoc" );
} // namespace EngineProperties

void Root::Startup()
{
	g_logManager = new LogManager();
	g_logManager->Startup();

	g_cvarManager = new CVarManager();
	g_cvarManager->Startup();

	g_projectManager = new ProjectManager();
	g_projectManager->Startup();

	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->Startup();

	g_entityDictionary = new EntityManager();
	g_entityDictionary->Startup();

	g_physicsManager = new PhysicsManager();
	g_physicsManager->Startup();

	g_inputManager = new InputManager();
	g_inputManager->Startup();

	g_renderManager = new RenderManager();
	g_renderManager->Startup();

	g_hostManager = new HostManager();
	g_hostManager->Startup();
}

void Root::Shutdown()
{
	g_hostManager->Shutdown();
	g_renderManager->Shutdown();
	g_inputManager->Shutdown();
	g_physicsManager->Shutdown();
	g_entityDictionary->Shutdown();
	g_renderdocManager->Shutdown();
	g_projectManager->Shutdown();
	g_cvarManager->Shutdown();
	g_logManager->Shutdown();

	_CrtDumpMemoryLeaks();
}

void Root::Run()
{
	g_renderManager->Run();
}