#pragma once
#include <vk_mem_alloc.h>

class RenderManager;
class RenderdocManager;
class HostManager;
class LogManager;

//
// Global vars
//
extern VmaAllocator* g_allocator;
extern RenderManager* g_renderManager;
extern LogManager* g_logManager;
extern HostManager* g_hostManager;
extern RenderdocManager* g_renderdocManager;

extern float g_curTime;
extern float g_frameTime;