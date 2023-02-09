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

	//
	// Managed bindings for things we want to access from C#
	//
	GENERATE_BINDINGS LogManager* GetLogManager() { return m_logManager; }
	GENERATE_BINDINGS EntityManager* GetEntityManager() { return m_entityManager; }
	GENERATE_BINDINGS InputManager* GetInputManager() { return m_inputManager; }
	GENERATE_BINDINGS CVarManager* GetCVarManager() { return m_cvarManager; }
	GENERATE_BINDINGS PhysicsManager* GetPhysicsManager() { return m_physicsManager; }

	// We aren't using these:
	// GENERATE_BINDINGS ProjectManager* GetProjectManager() { return m_projectManager; }
	// GENERATE_BINDINGS RenderManager* GetRenderManager() { return m_renderManager; }
	// GENERATE_BINDINGS RenderdocManager* GetRenderdocManager() { return m_renderdocManager; }
	// GENERATE_BINDINGS HostManager* GetHostManager() { return m_hostManager; }
	// GENERATE_BINDINGS BaseRenderContext* GetRenderContext() { return m_renderContext; }

	GENERATE_BINDINGS void Quit() { m_shouldQuit = true; }

	GENERATE_BINDINGS inline int GetCurrentTick() { return m_curTick; }
	GENERATE_BINDINGS inline float GetFrameDeltaTime() { return m_frameDeltaTime; }
	GENERATE_BINDINGS inline float GetTickDeltaTime() { return m_tickDeltaTime; }
	GENERATE_BINDINGS inline float GetFramesPerSecond() { return 1.0f / m_frameDeltaTime; }
	GENERATE_BINDINGS inline float GetTime() { return m_curTime; }

	GENERATE_BINDINGS inline bool IsServer() { return m_executingRealm == REALM_SERVER; }
	GENERATE_BINDINGS inline bool IsClient() { return m_executingRealm == REALM_CLIENT; }

	GENERATE_BINDINGS const char* GetProjectPath();

	GENERATE_BINDINGS uint32_t CreateBaseEntity();
	GENERATE_BINDINGS uint32_t CreateModelEntity();

	GENERATE_BINDINGS inline void SetCameraPosition( Vector3 position ) { m_cameraPos = position; }
	GENERATE_BINDINGS inline Vector3 GetCameraPosition() { return m_cameraPos; }

	GENERATE_BINDINGS inline void SetCameraRotation( Quaternion rotation ) { m_cameraRot = rotation; }
	GENERATE_BINDINGS inline Quaternion GetCameraRotation() { return m_cameraRot; }

	GENERATE_BINDINGS inline void SetCameraFieldOfView( float fov ) { m_cameraFov = fov; }
	GENERATE_BINDINGS inline float GetCameraFieldOfView() { return m_cameraFov; }

	GENERATE_BINDINGS inline void SetCameraZNear( float znear ) { m_cameraZNear = znear; }
	GENERATE_BINDINGS inline float GetCameraZNear() { return m_cameraZNear; }

	GENERATE_BINDINGS inline void SetCameraZFar( float zfar ) { m_cameraZFar = zfar; }
	GENERATE_BINDINGS inline float GetCameraZFar() { return m_cameraZFar; }
};
