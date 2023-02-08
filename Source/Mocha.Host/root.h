#pragma once
#include <defs.h>
#include <mathtypes.h>
#include <subsystem.h>

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

class Root
{
protected:
	bool m_shouldQuit = false;
	virtual bool GetQuitRequested() { return false; }

public:
	RenderManager* m_renderManager;
	LogManager* m_logManager;
	HostManager* m_hostManager;
	RenderdocManager* m_renderdocManager;
	EntityManager* m_entityManager;
	PhysicsManager* m_physicsManager;
	InputManager* m_inputManager;
	BaseRenderContext* m_renderContext;
	CVarManager* m_cvarManager;
	ProjectManager* m_projectManager;

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

	void Startup();
	void Run();
	void Shutdown();

	void Quit() { m_shouldQuit = true; }

	//
	// Managed bindings for things we want to access from C#
	//
	GENERATE_BINDINGS LogManager* GetLogManager() { return m_logManager; }
	GENERATE_BINDINGS EntityManager* GetEntityManager() { return m_entityManager; }
	GENERATE_BINDINGS InputManager* GetInputManager() { return m_inputManager; }
	GENERATE_BINDINGS CVarManager* GetCVarManager() { return m_cvarManager; }

	// We aren't using these:
	// GENERATE_BINDINGS ProjectManager* GetProjectManager() { return m_projectManager; }
	// GENERATE_BINDINGS RenderManager* GetRenderManager() { return m_renderManager; }
	// GENERATE_BINDINGS RenderdocManager* GetRenderdocManager() { return m_renderdocManager; }
	// GENERATE_BINDINGS HostManager* GetHostManager() { return m_hostManager; }
	// GENERATE_BINDINGS PhysicsManager* GetPhysicsManager() { return m_physicsManager; }
	// GENERATE_BINDINGS BaseRenderContext* GetRenderContext() { return m_renderContext; }
};
