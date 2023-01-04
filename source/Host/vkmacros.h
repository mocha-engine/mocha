#pragma once
#include <defs.h>
#include <vk_mem_alloc.h>
#include <volk.h>
#include <vulkan/vk_enum_string_helper.h>

#define VK_CHECK( x )                                                                                     \
	do                                                                                                    \
	{                                                                                                     \
		VkResult err = x;                                                                                 \
		if ( err )                                                                                        \
		{                                                                                                 \
			std::string result = std::string( "Vulkan error: " ) + std::string( string_VkResult( err ) ); \
			ErrorMessage( result );                                                                       \
			__debugbreak();                                                                               \
		}                                                                                                 \
	} while ( 0 )