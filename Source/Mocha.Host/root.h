#pragma once
#include <defs.h>
#include <subsystem.h>
#include <mathtypes.h>

class RenderManager;
class RenderdocManager;
class HostManager;
class LogManager;
class EntityManager;
class PhysicsManager;
class InputManager;
class BaseRenderContext;
class CVarManager;
class ProjectManager;

class Root : ISubSystem
{
protected:
	bool m_shouldQuit = false;
	virtual bool GetQuitRequested() = 0;

public:
	RenderManager* g_renderManager;
	LogManager* g_logManager;
	HostManager* g_hostManager;
	RenderdocManager* g_renderdocManager;
	EntityManager* g_entityDictionary;
	PhysicsManager* g_physicsManager;
	InputManager* g_inputManager;
	BaseRenderContext* g_renderContext;
	CVarManager* g_cvarManager;
	ProjectManager* g_projectManager;

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

	void Startup();
	void Run();
	void Shutdown();

	void Quit() { m_shouldQuit = true; }
};
