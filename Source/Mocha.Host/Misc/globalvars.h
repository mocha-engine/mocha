#pragma once

//
// Engine features
//
class StringCVar;
class BoolCVar;
class IntCVar;
class FloatCVar;

// TODO: move
namespace EngineProperties
{
	extern StringCVar LoadedProject;
	extern BoolCVar Raytracing;
	extern BoolCVar Renderdoc;

	extern StringCVar ServerHostname;
	extern StringCVar ServerPassword;
	extern IntCVar ServerPort;
	extern IntCVar ServerMaxPlayers;
}; // namespace EngineProperties

// TODO: Server / client
extern FloatCVar maxFramerate;

class RenderManager;
class LogManager;
class HostManager;
class RenderdocManager;
class EntityManager;
class PhysicsManager;
class EditorManager;
class InputManager;
class BaseRenderContext;
class CVarManager;
class ProjectManager;
class NetworkingManager;

struct Vector3;
struct Quaternion;
enum RenderDebugViews;
enum Realm;

namespace Globals
{
	extern RenderManager* m_renderManager;
	extern LogManager* m_logManager;
	extern HostManager* m_hostManager;
	extern RenderdocManager* m_renderdocManager;
	extern EntityManager* m_entityManager;
	extern PhysicsManager* m_physicsManager;
	extern EditorManager* m_editorManager;
	extern InputManager* m_inputManager;
	extern BaseRenderContext* m_renderContext;
	extern CVarManager* m_cvarManager;
	extern ProjectManager* m_projectManager;
	extern NetworkingManager* m_networkingManager;

	extern float m_curTime;
	extern float m_frameDeltaTime;
	extern float m_tickDeltaTime;
	extern int m_curTick;

	extern Vector3 m_cameraPos;
	extern Quaternion m_cameraRot;
	extern float m_cameraFov;
	extern float m_cameraZNear;
	extern float m_cameraZFar;

	extern RenderDebugViews m_debugView;
	extern Realm m_executingRealm;

	extern bool m_isDedicatedServer;
} // namespace Globals