#include "globalvars.h"

#include <cvarmanager.h>
#include <mathtypes.h>

HostManager* g_hostManager;
RenderManager* g_renderManager;
LogManager* g_logManager;
RenderdocManager* g_renderdocManager;
PhysicsManager* g_physicsManager;
InputManager* g_inputManager;
CVarManager* g_cvarManager;
ProjectManager* g_projectManager;

EntityManager* g_entityDictionary;

BaseRenderContext* g_renderContext; // TODO: Remove?

float g_curTime;
float g_frameDeltaTime;
float g_tickDeltaTime;
int g_curTick;

Vector3 g_cameraPos;
Quaternion g_cameraRot;
float g_cameraFov;
float g_cameraZNear;
float g_cameraZFar;

RenderDebugViews g_debugView;

Realm g_executingRealm;

//
// Engine features
//
namespace EngineProperties
{
	StringCVar LoadedProject(
	    "project.current", "Samples\\mocha-minimal\\project.json", CVarFlags::Archive, "Which project should we load?" );
	BoolCVar Raytracing( "render.raytracing", true, CVarFlags::Archive, "Enable raytracing" );
	BoolCVar Renderdoc( "render.renderdoc", false, CVarFlags::Archive, "Enable renderdoc" );

	StringCVar ServerName( "server.name", "Mocha Dedicated Server", CVarFlags::None, "Server name" );
	StringCVar ServerPassword( "server.password", "", CVarFlags::None, "Server password" );
	IntCVar ServerPort( "server.port", 7777, CVarFlags::None, "Server port" );
	IntCVar ServerMaxPlayers( "server.maxplayers", 16, CVarFlags::None, "Server max players" );
	FloatCVar timescale( "game.timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
} // namespace EngineProperties