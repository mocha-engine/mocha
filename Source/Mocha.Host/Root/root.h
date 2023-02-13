#pragma once
#include <Misc/defs.h>
#include <Misc/globalvars.h>
#include <Misc/mathtypes.h>
#include <Misc/subsystem.h>

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
class EditorManager;

class Root
{
protected:
	bool m_shouldQuit = false;
	virtual bool GetQuitRequested() { return false; }

public:
	void Startup();
	void Run();
	void Shutdown();

	//
	// Managed bindings for things we want to access from C#
	//
	GENERATE_BINDINGS LogManager* GetLogManager() { return Globals::m_logManager; }
	GENERATE_BINDINGS EntityManager* GetEntityManager() { return Globals::m_entityManager; }
	GENERATE_BINDINGS InputManager* GetInputManager() { return Globals::m_inputManager; }
	GENERATE_BINDINGS CVarManager* GetCVarManager() { return Globals::m_cvarManager; }
	GENERATE_BINDINGS PhysicsManager* GetPhysicsManager() { return Globals::m_physicsManager; }
	GENERATE_BINDINGS EditorManager* GetEditorManager() { return Globals::m_editorManager; }

	// We aren't using these:
	// GENERATE_BINDINGS ProjectManager* GetProjectManager() { return Globals::m_projectManager; }
	// GENERATE_BINDINGS RenderManager* GetRenderManager() { return Globals::m_renderManager; }
	// GENERATE_BINDINGS RenderdocManager* GetRenderdocManager() { return Globals::m_renderdocManager; }
	// GENERATE_BINDINGS HostManager* GetHostManager() { return Globals::m_hostManager; }
	// GENERATE_BINDINGS BaseRenderContext* GetRenderContext() { return Globals::m_renderContext; }

	GENERATE_BINDINGS void Quit() { m_shouldQuit = true; }

	GENERATE_BINDINGS inline int GetCurrentTick() { return Globals::m_curTick; }
	GENERATE_BINDINGS inline float GetFrameDeltaTime() { return Globals::m_frameDeltaTime; }
	GENERATE_BINDINGS inline float GetTickDeltaTime() { return Globals::m_tickDeltaTime; }
	GENERATE_BINDINGS inline float GetFramesPerSecond() { return 1.0f / Globals::m_frameDeltaTime; }
	GENERATE_BINDINGS inline float GetTime() { return Globals::m_curTime; }

	GENERATE_BINDINGS inline bool IsServer() { return Globals::m_executingRealm == REALM_SERVER; }
	GENERATE_BINDINGS inline bool IsClient() { return Globals::m_executingRealm == REALM_CLIENT; }

	GENERATE_BINDINGS const char* GetProjectPath();

	GENERATE_BINDINGS uint32_t CreateBaseEntity();
	GENERATE_BINDINGS uint32_t CreateModelEntity();

	GENERATE_BINDINGS inline void SetCameraPosition( Vector3 position ) { Globals::m_cameraPos = position; }
	GENERATE_BINDINGS inline Vector3 GetCameraPosition() { return Globals::m_cameraPos; }

	GENERATE_BINDINGS inline void SetCameraRotation( Quaternion rotation ) { Globals::m_cameraRot = rotation; }
	GENERATE_BINDINGS inline Quaternion GetCameraRotation() { return Globals::m_cameraRot; }

	GENERATE_BINDINGS inline void SetCameraFieldOfView( float fov ) { Globals::m_cameraFov = fov; }
	GENERATE_BINDINGS inline float GetCameraFieldOfView() { return Globals::m_cameraFov; }

	GENERATE_BINDINGS inline void SetCameraZNear( float znear ) { Globals::m_cameraZNear = znear; }
	GENERATE_BINDINGS inline float GetCameraZNear() { return Globals::m_cameraZNear; }

	GENERATE_BINDINGS inline void SetCameraZFar( float zfar ) { Globals::m_cameraZFar = zfar; }
	GENERATE_BINDINGS inline float GetCameraZFar() { return Globals::m_cameraZFar; }

	GENERATE_BINDINGS void CreateListenServer();

	GENERATE_BINDINGS Vector2 GetWindowSize();
	GENERATE_BINDINGS Vector2 GetRenderSize();
};
