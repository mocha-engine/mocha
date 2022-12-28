#pragma once
#include <vk_mem_alloc.h>

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

class RenderManager;
class RenderdocManager;
class HostManager;
class LogManager;
class EntityManager;
class CVarManager;
class PhysicsManager;
class InputManager;

struct GameSettings;

struct Vector3;
struct Quaternion;

//
// Global vars
//
extern GameSettings* g_gameSettings;
extern VmaAllocator* g_allocator;
extern RenderManager* g_renderManager;
extern LogManager* g_logManager;
extern HostManager* g_hostManager;
extern RenderdocManager* g_renderdocManager;
extern EntityManager* g_entityDictionary;
extern CVarManager* g_cvarManager;
extern PhysicsManager* g_physicsManager;
extern InputManager* g_inputManager;

extern float g_curTime;
extern float g_frameTime;

extern Vector3 g_cameraPos;
extern Quaternion g_cameraRot;
extern float g_cameraFov;
extern float g_cameraZNear;
extern float g_cameraZFar;

extern RenderDebugViews g_debugView;