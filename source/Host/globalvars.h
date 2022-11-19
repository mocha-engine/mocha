#pragma once
#include <vk_mem_alloc.h>

class NativeEngine;

namespace Global
{
	extern VmaAllocator* g_allocator;
	extern NativeEngine* g_engine;
}; // namespace Global