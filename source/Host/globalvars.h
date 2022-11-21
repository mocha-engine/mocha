#pragma once
#include <vk_mem_alloc.h>

class NativeEngine;
class Renderdoc;
class ManagedHost;
class Logger;

//
// Global vars
//
extern VmaAllocator* g_allocator;
extern NativeEngine* g_engine;
extern Logger* g_logger;
extern ManagedHost* g_managedHost;
extern Renderdoc* g_renderdoc;

extern float g_curTime;
extern float g_frameTime;