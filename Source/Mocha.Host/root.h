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

class Root
{
protected:
	bool m_shouldQuit = false;
	virtual bool GetQuitRequested() = 0;

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
};
