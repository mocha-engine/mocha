#pragma once

#include <VkBootstrap.h>
#include <baserendercontext.h>
#include <defs.h>
#include <game_types.h>
#include <globalvars.h>
#include <handlemap.h>
#include <vkinit.h>
#include <vulkan/vulkan.h>
#include <window.h>

#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>

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

class VulkanImageTexture
{
public:
	VkImage image;
	VmaAllocation allocation;
	VkImageView imageView;
	VkFormat format;

	VulkanImageTexture() {}
	VulkanImageTexture( VkDevice device, Size2D size );
};

// ----------------------------------------------------------------------------------------------------

class VulkanSwapchain
{
private:
	void CreateMainSwapchain( VkDevice device, VkPhysicalDevice physicalDevice, VkSurfaceKHR surface, Size2D size );
	void CreateDepthTexture( VkDevice device, Size2D size );

public:
	VkSwapchainKHR m_swapchain;
	std::vector<VulkanRenderTexture> m_swapchainTextures;
	VulkanRenderTexture m_depthTexture;

	uint32_t AcquireSwapchainImageIndex( VkDevice device, VkSemaphore presentSemaphore, VulkanCommandContext mainContext )
	{
		uint32_t swapchainImageIndex;

		VK_CHECK( vkAcquireNextImageKHR( device, m_swapchain, 1000000000, presentSemaphore, nullptr, &swapchainImageIndex ) );
		VK_CHECK( vkResetCommandBuffer( mainContext.commandBuffer, 0 ) );

		return swapchainImageIndex;
	}

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

	//
	// State
	//
	uint32_t m_swapchainImageIndex;
	// Current swapchain target image. Refers to m_swapchain.images[currentImageIndex]
	VulkanRenderTexture m_swapchainTarget;
	// Current swapchain target depth buffer.
	VulkanRenderTexture m_depthTarget;

	// Checks to see whether the current window size is valid for rendering.
	inline bool CanRender();

	//
	// Handle maps
	// We refer to these in our external 'base' objects..
	// struct Texture
	// {
	//    public Handle handle;
	//
	//    Texture( /* ... */ )
	//    {
	//         handle = RenderContext::CreateImageTexture( /* ... */ );
	//    }
	// }
	//
	HandleMap<VkBuffer> m_buffers = {};
	HandleMap<VulkanImageTexture> m_imageTextures = {};
	HandleMap<VulkanRenderTexture> m_renderTextures = {};

public:
	RenderContextStatus Startup() override;
	RenderContextStatus Shutdown() override;
	RenderContextStatus BeginRendering() override;
	RenderContextStatus EndRendering() override;
	// ----------------------------------------
	RenderContextStatus BindPipeline( Pipeline p ) override;

	RenderContextStatus BindDescriptor( Descriptor d ) override;

	RenderContextStatus BindVertexBuffer( VertexBuffer vb ) override;

	RenderContextStatus BindIndexBuffer( IndexBuffer ib ) override;

	RenderContextStatus Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount ) override;

	RenderContextStatus BindRenderTarget( RenderTexture rt ) override;
	// ----------------------------------------
	RenderContextStatus RenderEntity( BaseEntity* entity ) override;
};
