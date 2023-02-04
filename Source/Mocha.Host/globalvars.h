#pragma once

// TODO: Remove
enum RenderDebugViews
{
	NONE = 0,
	DIFFUSE = 1,
	NORMAL = 2,
	AMBIENTOCCLUSION = 3,
	METALNESS = 4,
	ROUGHNESS = 5,

	OTHER = 63
};

enum Realm
{
	REALM_SERVER,
	REALM_CLIENT
};

inline const char* RealmToString( const Realm& realm )
{
	switch ( realm )
	{
	case REALM_SERVER:
		return "Server";
	case REALM_CLIENT:
		return "Client";
	}

	__debugbreak();
	return "Unknown";
}

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

struct Vector3;
struct Quaternion;

//
// Global vars
//
extern RenderManager* g_renderManager;
extern LogManager* g_logManager;
extern HostManager* g_hostManager;
extern RenderdocManager* g_renderdocManager;
extern EntityManager* g_entityDictionary;
extern PhysicsManager* g_physicsManager;
extern InputManager* g_inputManager;
extern BaseRenderContext* g_renderContext;
extern CVarManager* g_cvarManager;
extern ProjectManager* g_projectManager;

extern float g_curTime;
extern float g_frameDeltaTime;
extern float g_tickDeltaTime;
extern int g_curTick;

extern Vector3 g_cameraPos;
extern Quaternion g_cameraRot;
extern float g_cameraFov;
extern float g_cameraZNear;
extern float g_cameraZFar;

extern RenderDebugViews g_debugView;
extern Realm g_executingRealm;