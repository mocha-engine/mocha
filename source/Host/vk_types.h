#pragma once
#include <vk_mem_alloc.h>
#include <volk.h>

struct AllocatedBuffer
{
	VkBuffer buffer;
	VmaAllocation allocation;
};

struct AllocatedImage
{
	VkImage image;
	VmaAllocation allocation;
};

struct UploadContext
{
	VkFence uploadFence;
	VkCommandPool commandPool;
	VkCommandBuffer commandBuffer;
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