#include "globalvars.h"

#include <Misc/cvarmanager.h>
#include <Misc/mathtypes.h>
#include <Root/clientroot.h>
#include <Root/root.h>

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

namespace Globals
{
	RenderManager* m_renderManager;
	LogManager* m_logManager;
	HostManager* m_hostManager;
	RenderdocManager* m_renderdocManager;
	EntityManager* m_entityManager;
	PhysicsManager* m_physicsManager;
	EditorManager* m_editorManager;
	InputManager* m_inputManager;
	BaseRenderContext* m_renderContext;
	CVarManager* m_cvarManager;
	ProjectManager* m_projectManager;
	NetworkingManager* m_networkingManager;

	float m_curTime;
	float m_frameDeltaTime;
	float m_tickDeltaTime;
	int m_curTick;

	Vector3 m_cameraPos;
	Quaternion m_cameraRot;
	float m_cameraFov;
	float m_cameraZNear;
	float m_cameraZFar;

	RenderDebugViews m_debugView;
	Realm m_executingRealm;

	bool m_isDedicatedServer;

	char* m_activeProjectPath;
} // namespace Globals