#pragma once
#include <vk_mem_alloc.h>

class CNativeEngine;

namespace Global
{
	extern VmaAllocator* g_allocator;
	extern CNativeEngine* g_engine;
}; // namespace Global