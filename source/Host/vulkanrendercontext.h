#pragma once

#include <VkBootstrap.h>
#include <baserendercontext.h>
#include <defs.h>
#include <game_types.h>
#include <globalvars.h>
#include <vulkan/vulkan.h>
#include <window.h>

#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>
#include <vkinit.h>

// ----------------------------------------------------------------------------------------------------

enum RenderTextureType
{
	RENDER_TEXTURE_COLOR,
	RENDER_TEXTURE_COLOR_OPAQUE,
	RENDER_TEXTURE_DEPTH
};

// ----------------------------------------------------------------------------------------------------

enum SamplerType
{
	SAMPLER_TYPE_POINT,
	SAMPLER_TYPE_LINEAR,
	SAMPLER_TYPE_ANISOTROPIC
};

// ----------------------------------------------------------------------------------------------------

struct VulkanBuffer
{
	VkBuffer buffer;
	VmaAllocation allocation;

	VulkanBuffer() {}
	VulkanBuffer( VkDevice m_device, size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage,
	    VmaAllocationCreateFlagBits allocFlags );
};

// ----------------------------------------------------------------------------------------------------

struct VulkanSampler
{
private:
	VkSamplerCreateInfo GetCreateInfo( SamplerType samplerType );

public:
	VkSampler sampler;

	VulkanSampler() {}
	VulkanSampler( VkDevice m_device, SamplerType samplerType );
};

// ----------------------------------------------------------------------------------------------------

struct VulkanCommandContext
{
	VkCommandPool commandPool;
	VkCommandBuffer commandBuffer;
	VkFence fence;

	VulkanCommandContext() {}
	VulkanCommandContext( VkDevice device, uint32_t graphicsQueueFamily );
};

// ----------------------------------------------------------------------------------------------------

class VulkanRenderTexture
{
private:
	VkImageUsageFlagBits GetUsageFlagBits( RenderTextureType type );
	VkFormat GetFormat( RenderTextureType type );
	VkImageAspectFlags GetAspectFlags( RenderTextureType type );

public:
	VkImage image;
	VmaAllocation allocation;
	VkImageView imageView;
	VkFormat format;

	VulkanRenderTexture() {}
	VulkanRenderTexture( VkDevice device, Size2D size, RenderTextureType type );
};

// ----------------------------------------------------------------------------------------------------

class VulkanSwapchain
{
private:
	void CreateMainSwapchain( VkDevice device, VkPhysicalDevice physicalDevice, VkSurfaceKHR surface, Size2D size );
	void CreateDepthTexture( VkDevice device, Size2D size );

public:
	VkSwapchainKHR m_swapchain;
	std::vector<VkImage> m_images;
	std::vector<VkImageView> m_imageViews;
	VkFormat m_imageFormat;

	VulkanRenderTexture m_depthTexture;

	VulkanSwapchain() {}
	VulkanSwapchain( VkDevice device, VkPhysicalDevice physicalDevice, VkSurfaceKHR surface, Size2D size );
};

// ----------------------------------------------------------------------------------------------------

class VulkanRenderContext : public BaseRenderContext
{
private:
	//
	// Vulkan-specifics
	//
	VkDebugUtilsMessengerEXT m_debugMessenger;
	VkPhysicalDevice m_chosenGPU;
	VkDevice m_device;
	VkInstance m_instance;
	VkPhysicalDeviceProperties m_deviceProperties;
	VkQueue m_graphicsQueue;
	uint32_t m_graphicsQueueFamily;
	VkSurfaceKHR m_surface;
	VkSemaphore m_presentSemaphore, m_renderSemaphore;
	VkDescriptorPool m_descriptorPool;

	std::unique_ptr<Window> m_window;
	VulkanCommandContext m_mainContext;
	VulkanCommandContext m_uploadContext;
	VulkanSwapchain m_swapchain;
	VulkanSampler m_anisoSampler, m_pointSampler;

	// Create a Vulkan context, set up devices
	vkb::Instance CreateInstanceAndSurface();
	void FinalizeAndCreateDevice( vkb::PhysicalDevice physicalDevice );
	vkb::PhysicalDevice CreatePhysicalDevice( vkb::Instance vkbInstance );

	void CreateSwapchain();
	void CreateCommands();
	void CreateSyncStructures();
	void CreateDescriptors();
	void CreateSamplers();

	//
	// Vulkan memory allocator
	//
	VmaAllocator m_allocator;
	void CreateAllocator();

public:
	RenderContextStatus Startup() override;
	RenderContextStatus Shutdown() override;
	RenderContextStatus BeginRendering() override;
	RenderContextStatus EndRendering() override;
};
