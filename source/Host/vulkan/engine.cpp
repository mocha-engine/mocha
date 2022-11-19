#include "engine.h"

#include "../managed/ManagedHost.h"
#include "../thirdparty/VkBootstrap.h"
#include "../window.h"
#include "mesh.h"
#include "shadercompiler.h"
#include "types.h"
#include "vkinit.h"

#include <fstream>
#include <glm/ext.hpp>
#include <iostream>
#include <memory>
#include <spdlog/spdlog.h>

#define VMA_IMPLEMENTATION
#include <globalvars.h>
#include <vk_mem_alloc.h>

namespace Global
{
	VmaAllocator* g_allocator;
	NativeEngine* g_engine;
} // namespace Global

VkBool32 DebugCallback( VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageTypes,
    const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData )
{
	const std::shared_ptr<spdlog::logger> logger = spdlog::get( "renderer" );

	switch ( messageSeverity )
	{
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT:
		logger->trace( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT:
		logger->info( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT:
		logger->warn( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT:
		logger->error( pCallbackData->pMessage );
		break;
	}

	return VK_FALSE;
}

void NativeEngine::InitVulkan()
{
	vkb::InstanceBuilder builder;

	auto ret = builder.set_app_name( "Mocha VK" )
	               .set_engine_name( "Mocha" )
	               .request_validation_layers( true )
	               .require_api_version( 1, 3, 0 )
	               .set_debug_callback( &DebugCallback )
	               .build();

	vkb::Instance vkbInstance = ret.value();

	m_instance = vkbInstance.instance;
	m_debugMessenger = vkbInstance.debug_messenger;

	m_surface = m_window->CreateSurface( m_instance );

	vkb::PhysicalDeviceSelector selector( vkbInstance );

	VkPhysicalDeviceVulkan13Features requiredFeatures = {};
	requiredFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_3_FEATURES;
	requiredFeatures.pNext = nullptr;
	requiredFeatures.dynamicRendering = true;
	selector.set_required_features_13( requiredFeatures );

	vkb::PhysicalDevice physicalDevice = selector.set_minimum_version( 1, 3 ).set_surface( m_surface ).select().value();

	vkb::DeviceBuilder deviceBuilder( physicalDevice );
	vkb::Device vkbDevice = deviceBuilder.build().value();

	m_device = vkbDevice.device;
	m_chosenGPU = vkbDevice.physical_device;

	m_graphicsQueue = vkbDevice.get_queue( vkb::QueueType::graphics ).value();
	m_graphicsQueueFamily = vkbDevice.get_queue_index( vkb::QueueType::graphics ).value();

	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.physicalDevice = m_chosenGPU;
	allocatorInfo.device = m_device;
	allocatorInfo.instance = m_instance;
	vmaCreateAllocator( &allocatorInfo, &m_allocator );
}

void NativeEngine::InitSwapchain()
{
	vkb::SwapchainBuilder swapchainBuilder( m_chosenGPU, m_device, m_surface );

	vkb::Swapchain vkbSwapchain =
	    swapchainBuilder.set_desired_format( { VK_FORMAT_R8G8B8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	        .set_desired_present_mode( VK_PRESENT_MODE_FIFO_KHR )
	        .set_desired_extent( m_windowExtent.width, m_windowExtent.height )
	        .build()
	        .value();

	m_swapchain = vkbSwapchain.swapchain;
	m_swapchainImages = vkbSwapchain.get_images().value();
	m_swapchainImageViews = vkbSwapchain.get_image_views().value();
	m_swapchainImageFormat = vkbSwapchain.image_format;

	VkExtent3D depthImageExtent = {
	    m_windowExtent.width,
	    m_windowExtent.height,
	    1,
	};

	m_depthFormat = VK_FORMAT_D32_SFLOAT_S8_UINT; // Depth & stencil format

	VkImageCreateInfo depthImageInfo =
	    VKInit::ImageCreateInfo( m_depthFormat, VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT, depthImageExtent );

	VmaAllocationCreateInfo depthAllocInfo = {};
	depthAllocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	depthAllocInfo.requiredFlags = VkMemoryPropertyFlags( VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT );

	vmaCreateImage( m_allocator, &depthImageInfo, &depthAllocInfo, &m_depthImage.image, &m_depthImage.allocation, nullptr );

	VkImageViewCreateInfo depthViewInfo =
	    VKInit::ImageViewCreateInfo( m_depthFormat, m_depthImage.image, VK_IMAGE_ASPECT_DEPTH_BIT );

	VK_CHECK( vkCreateImageView( m_device, &depthViewInfo, nullptr, &m_depthImageView ) );
}

void NativeEngine::InitCommands()
{
	VkCommandPoolCreateInfo commandPoolInfo = {};
	commandPoolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	commandPoolInfo.pNext = nullptr;

	commandPoolInfo.queueFamilyIndex = m_graphicsQueueFamily;
	commandPoolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

	VK_CHECK( vkCreateCommandPool( m_device, &commandPoolInfo, nullptr, &m_commandPool ) );

	VkCommandBufferAllocateInfo commandAllocInfo = {};
	commandAllocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	commandAllocInfo.pNext = nullptr;

	commandAllocInfo.commandPool = m_commandPool;
	commandAllocInfo.commandBufferCount = 1;
	commandAllocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

	VK_CHECK( vkAllocateCommandBuffers( m_device, &commandAllocInfo, &m_commandBuffer ) );
}

void NativeEngine::InitSyncStructures()
{
	VkFenceCreateInfo fenceCreateInfo = {};
	fenceCreateInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceCreateInfo.pNext = nullptr;

	fenceCreateInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

	VK_CHECK( vkCreateFence( m_device, &fenceCreateInfo, nullptr, &m_renderFence ) );

	VkSemaphoreCreateInfo semaphoreCreateInfo = {};
	semaphoreCreateInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreCreateInfo.pNext = nullptr;
	semaphoreCreateInfo.flags = 0;

	VK_CHECK( vkCreateSemaphore( m_device, &semaphoreCreateInfo, nullptr, &m_presentSemaphore ) );
	VK_CHECK( vkCreateSemaphore( m_device, &semaphoreCreateInfo, nullptr, &m_renderSemaphore ) );
}

void NativeEngine::Init()
{
	m_window = std::make_unique<Window>( Window( m_windowExtent.width, m_windowExtent.height ) );

	InitVulkan();

	// Set up global vars
	Global::g_engine = this;
	Global::g_allocator = &m_allocator;

	InitSwapchain();
	InitCommands();
	InitSyncStructures();

	m_camera = new Camera();

	m_triangle = new Model();
	m_triangle->InitPipelines();
	m_triangle->UploadTriangleMesh();

	m_isInitialized = true;
}

void NativeEngine::Cleanup()
{
	if ( m_isInitialized )
	{
		vkDestroyCommandPool( m_device, m_commandPool, nullptr );

		vkDestroySwapchainKHR( m_device, m_swapchain, nullptr );

		for ( size_t i = 0; i < m_swapchainImageViews.size(); i++ )
		{
			vkDestroyImageView( m_device, m_swapchainImageViews[i], nullptr );
		}

		vkDestroyDevice( m_device, nullptr );
		vkDestroySurfaceKHR( m_instance, m_surface, nullptr );

		vkb::destroy_debug_utils_messenger( m_instance, m_debugMessenger );
		vkDestroyInstance( m_instance, nullptr );

		m_window->Cleanup();
	}
}

void NativeEngine::Render()
{
	// Wait until we can render ( 1 second timeout )
	VK_CHECK( vkWaitForFences( m_device, 1, &m_renderFence, true, 1000000000 ) );
	VK_CHECK( vkResetFences( m_device, 1, &m_renderFence ) );

	// Acquire swapchain image ( 1 second timeout )
	uint32_t swapchainImageIndex;
	VK_CHECK( vkAcquireNextImageKHR( m_device, m_swapchain, 1000000000, m_presentSemaphore, nullptr,
	    &swapchainImageIndex ) ); // TODO: Check for VK_ERROR_OUT_OF_DATE_KHR or VK_SUBOPTIMAL_KHR and resize
	VK_CHECK( vkResetCommandBuffer( m_commandBuffer, 0 ) );

	// Begin command buffer
	VkCommandBuffer cmd = m_commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo();
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	// Dynamic rendering
	VkImageView currentImageView = m_swapchainImageViews[swapchainImageIndex];

	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkClearValue depthClear = {};
	depthClear.depthStencil.depth = 1.0f;

	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( currentImageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, colorClear );

	VkRenderingAttachmentInfo depthAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_depthImageView, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL, depthClear );

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( colorAttachmentInfo, depthAttachmentInfo, m_windowExtent );

	// Start drawing
	vkCmdBeginRendering( cmd, &renderInfo );

	m_triangle->Render( m_camera, cmd, m_frameNumber );

	// End drawing
	vkCmdEndRendering( cmd );
	VK_CHECK( vkEndCommandBuffer( cmd ) );

	// Submit
	VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	VkSubmitInfo submit = VKInit::SubmitInfo( &waitStage, &m_presentSemaphore, &m_renderSemaphore, &cmd );
	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, m_renderFence ) );

	// Present
	VkPresentInfoKHR presentInfo = VKInit::PresentInfo( &m_swapchain, &m_renderSemaphore, &swapchainImageIndex );
	VK_CHECK( vkQueuePresentKHR(
	    m_graphicsQueue, &presentInfo ) ); // TODO: Check for VK_ERROR_OUT_OF_DATE_KHR or VK_SUBOPTIMAL_KHR and resize

	m_frameNumber++;
}

void NativeEngine::Run( ManagedHost* managedHost )
{
	bool bQuit = false;

	while ( !bQuit )
	{
		bQuit = m_window->Update();

		managedHost->Invoke( "Render" );
		m_camera->Update( m_frameNumber );

		Render();
	}
}

void NativeEngine::SetCamera( Camera* camera )
{
	m_camera = camera;
}
