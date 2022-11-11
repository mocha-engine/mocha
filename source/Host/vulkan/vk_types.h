#pragma once
#include <vk_mem_alloc.h>
#include <vulkan/vulkan.h>

struct AllocatedBuffer
{
	VkBuffer buffer;
	VmaAllocation allocation;
};

#define VK_CHECK( x )                                          \
	do                                                         \
	{                                                          \
		VkResult err = x;                                      \
		if ( err )                                             \
		{                                                      \
			printf( "Detected vulkan error: %d", ( int )err ); \
			abort();                                           \
		}                                                      \
	} while ( 0 )