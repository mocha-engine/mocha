#include "rendermanager.h"

#include <VkBootstrap.h>
#include <defs.h>
#include <fstream>
#include <glm/ext.hpp>
#include <hostmanager.h>
#include <iostream>
#include <memory>
#include <mesh.h>
#include <shadercompiler.h>
#include <spdlog/spdlog.h>
#include <vk_types.h>
#include <vkinit.h>
#include <window.h>

#ifdef _IMGUI
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <imgui.h>
#endif

#define VMA_IMPLEMENTATION
#include <baseentity.h>
#include <cvarmanager.h>
#include <edict.h>
#include <globalvars.h>
#include <modelentity.h>
#include <physicsmanager.h>
#include <vk_mem_alloc.h>

FloatCVar timescale( "timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );

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

void RenderManager::InitVulkan()
{
	vkb::InstanceBuilder builder;

	auto ret = builder.set_app_name( GAME_NAME )
	               .set_engine_name( ENGINE_NAME )
	               .request_validation_layers( true )
	               .require_api_version( 1, 3, 0 )
	               .set_debug_callback( &DebugCallback )
	               .build();

	vkb::Instance vkbInstance = ret.value();

	m_instance = vkbInstance.instance;
	m_debugMessenger = vkbInstance.debug_messenger;

	m_surface = m_window->CreateSurface( m_instance );

	vkb::PhysicalDeviceSelector selector( vkbInstance );

	//
	// Set required VK1.0 features
	//
	VkPhysicalDeviceFeatures requiredFeatures = {};
	requiredFeatures.samplerAnisotropy = VK_TRUE;
	selector.set_required_features( requiredFeatures );

	//
	// Set required VK1.3 features
	//
	VkPhysicalDeviceVulkan13Features requiredFeatures13 = {};
	requiredFeatures13.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_3_FEATURES;
	requiredFeatures13.pNext = nullptr;
	requiredFeatures13.dynamicRendering = true;
	selector.set_required_features_13( requiredFeatures13 );

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

void RenderManager::InitDeviceProperties()
{
	VkPhysicalDeviceProperties deviceProperties = {};
	vkGetPhysicalDeviceProperties( m_chosenGPU, &deviceProperties );

	m_deviceName = deviceProperties.deviceName;
}

void RenderManager::InitSwapchain()
{
	CreateSwapchain( GetWindowExtent() );
}

VkExtent2D RenderManager::GetWindowExtent()
{
	int windowWidth, windowHeight;
	m_window->GetWindowSize( &windowWidth, &windowHeight );
	VkExtent2D windowExtent = { windowWidth, windowHeight };

	return windowExtent;
}

void RenderManager::CreateSwapchain( VkExtent2D size )
{
	vkb::SwapchainBuilder swapchainBuilder( m_chosenGPU, m_device, m_surface );

	vkb::Swapchain vkbSwapchain = swapchainBuilder.set_old_swapchain( m_swapchain )
	                                  .set_desired_format( { VK_FORMAT_R8G8B8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	                                  .set_desired_present_mode( VK_PRESENT_MODE_FIFO_KHR )
	                                  .set_desired_extent( size.width, size.height )
	                                  .build()
	                                  .value();

	m_swapchain = vkbSwapchain.swapchain;
	m_swapchainImages = vkbSwapchain.get_images().value();
	m_swapchainImageViews = vkbSwapchain.get_image_views().value();
	m_swapchainImageFormat = vkbSwapchain.image_format;

	VkExtent3D depthImageExtent = {
	    size.width,
	    size.height,
	    1,
	};

	m_depthFormat = VK_FORMAT_D32_SFLOAT_S8_UINT; // Depth & stencil format

	VkImageCreateInfo depthImageInfo =
	    VKInit::ImageCreateInfo( m_depthFormat, VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT, depthImageExtent, 1 );

	VmaAllocationCreateInfo depthAllocInfo = {};
	depthAllocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	depthAllocInfo.requiredFlags = VkMemoryPropertyFlags( VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT );

	vmaCreateImage( m_allocator, &depthImageInfo, &depthAllocInfo, &m_depthImage.image, &m_depthImage.allocation, nullptr );

	VkImageViewCreateInfo depthViewInfo =
	    VKInit::ImageViewCreateInfo( m_depthFormat, m_depthImage.image, VK_IMAGE_ASPECT_DEPTH_BIT, 1 );

	VK_CHECK( vkCreateImageView( m_device, &depthViewInfo, nullptr, &m_depthImageView ) );
}

void RenderManager::InitCommands()
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

	// create pool for upload context
	VK_CHECK( vkCreateCommandPool( m_device, &commandPoolInfo, nullptr, &m_uploadContext.commandPool ) );
	commandAllocInfo.commandPool = m_uploadContext.commandPool;
	VK_CHECK( vkAllocateCommandBuffers( m_device, &commandAllocInfo, &m_uploadContext.commandBuffer ) );
}

void RenderManager::InitSyncStructures()
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

	VkFenceCreateInfo uploadFenceCreateInfo = {};
	uploadFenceCreateInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceCreateInfo.pNext = nullptr;

	fenceCreateInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

	VK_CHECK( vkCreateFence( m_device, &fenceCreateInfo, nullptr, &m_uploadContext.uploadFence ) );
	vkResetFences( m_device, 1, &m_uploadContext.uploadFence );
}

void RenderManager::InitImGUI()
{
#ifdef _IMGUI

	VkDescriptorPoolSize pool_sizes[] = { { VK_DESCRIPTOR_TYPE_SAMPLER, 1000 },
	    { VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, 1000 }, { VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_IMAGE, 1000 }, { VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER, 1000 }, { VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 1000 }, { VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC, 1000 }, { VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT, 1000 } };

	VkDescriptorPoolCreateInfo pool_info = {};
	pool_info.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
	pool_info.flags = VK_DESCRIPTOR_POOL_CREATE_FREE_DESCRIPTOR_SET_BIT;
	pool_info.maxSets = 1000;
	pool_info.poolSizeCount = ( uint32_t )std::size( pool_sizes );
	pool_info.pPoolSizes = pool_sizes;

	VkDescriptorPool imguiPool;
	VK_CHECK( vkCreateDescriptorPool( m_device, &pool_info, nullptr, &imguiPool ) );

	ImGui::CreateContext();

	ImGui_ImplSDL2_InitForVulkan( m_window->GetSDLWindow() );

	ImGui_ImplVulkan_InitInfo init_info = {};
	init_info.Instance = m_instance;
	init_info.PhysicalDevice = m_chosenGPU;
	init_info.Device = m_device;
	init_info.Queue = m_graphicsQueue;
	init_info.DescriptorPool = imguiPool;
	init_info.MinImageCount = 3;
	init_info.ImageCount = 3;
	init_info.MSAASamples = VK_SAMPLE_COUNT_1_BIT;
	init_info.UseDynamicRendering = true;
	init_info.ColorAttachmentFormat = m_swapchainImageFormat;

	ImGui_ImplVulkan_Init( &init_info, nullptr );
	ImmediateSubmit( [&]( VkCommandBuffer cmd ) { ImGui_ImplVulkan_CreateFontsTexture( cmd ); } );
	ImGui_ImplVulkan_DestroyFontUploadObjects();
#endif
}

void RenderManager::InitDescriptors()
{
	VkDescriptorPoolSize poolSizes[] = { { VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1000 },
	    { VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, 1000 }, { VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_IMAGE, 1000 }, { VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER, 1000 }, { VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC, 1000 },
	    { VK_DESCRIPTOR_TYPE_STORAGE_BUFFER_DYNAMIC, 1000 }, { VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT, 1000 } };

	VkDescriptorPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
	poolInfo.pNext = nullptr;
	poolInfo.flags = VK_DESCRIPTOR_POOL_CREATE_FREE_DESCRIPTOR_SET_BIT;
	poolInfo.maxSets = 1000;
	poolInfo.poolSizeCount = ( uint32_t )std::size( poolSizes );
	poolInfo.pPoolSizes = poolSizes;

	VK_CHECK( vkCreateDescriptorPool( m_device, &poolInfo, nullptr, &m_descriptorPool ) );
}

void RenderManager::InitSamplers()
{
	VkSamplerCreateInfo samplerInfo = VKInit::SamplerCreateInfo( VK_FILTER_LINEAR, VK_SAMPLER_ADDRESS_MODE_REPEAT, true );
	VK_CHECK( vkCreateSampler( g_renderManager->m_device, &samplerInfo, nullptr, &m_anisoSampler ) );

	samplerInfo = VKInit::SamplerCreateInfo( VK_FILTER_NEAREST, VK_SAMPLER_ADDRESS_MODE_REPEAT, true );
	VK_CHECK( vkCreateSampler( g_renderManager->m_device, &samplerInfo, nullptr, &m_pointSampler ) );
}

void RenderManager::Startup()
{
	m_window = std::make_unique<Window>( Window( 1280, 720 ) );
	m_window->m_onWindowResized = [this]( VkExtent2D newWindowExtents ) {
		g_hostManager->FireEvent( "Event.Window.Resized" );
		CreateSwapchain( newWindowExtents );
	};

	InitVulkan();

	// Set up global vars
	g_renderManager = this;
	g_allocator = &m_allocator;

	InitDeviceProperties();
	InitSwapchain();
	InitCommands();
	InitSyncStructures();
	InitDescriptors();
	InitSamplers();
	InitImGUI();

	m_isInitialized = true;
}

void RenderManager::Shutdown()
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

void RenderManager::Render()
{
#ifdef _IMGUI
	ImGui::Render();
#endif

	// Get window size ( we use this in a load of places )
	VkExtent2D windowExtent = GetWindowExtent();

	if ( windowExtent.width < 1 || windowExtent.height < 1 )
	{
		// Do not render if we can't render to anything..
		return;
	}

	// Wait until we can render ( 1 second timeout )
	VK_CHECK( vkWaitForFences( m_device, 1, &m_renderFence, true, 1000000000 ) );
	VK_CHECK( vkResetFences( m_device, 1, &m_renderFence ) );

	// Acquire swapchain image ( 1 second timeout )
	uint32_t swapchainImageIndex;
	VK_CHECK( vkAcquireNextImageKHR( m_device, m_swapchain, 1000000000, m_presentSemaphore, nullptr, &swapchainImageIndex ) );
	VK_CHECK( vkResetCommandBuffer( m_commandBuffer, 0 ) );

	// Begin command buffer
	VkCommandBuffer cmd = m_commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo( VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT );
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	//
	// Set viewport & scissor
	//
	VkViewport viewport = {};
	viewport.minDepth = 0.0;
	viewport.maxDepth = 1.0;
	viewport.width = windowExtent.width;
	viewport.height = windowExtent.height;

	VkRect2D scissor = { { 0, 0 }, { windowExtent.width, windowExtent.height } };
	vkCmdSetScissor( cmd, 0, 1, &scissor );
	vkCmdSetViewport( cmd, 0, 1, &viewport );

	//
	// We want to draw the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL before presenting
	//
	VkImageMemoryBarrier startRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier(
	    VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, m_swapchainImages[swapchainImageIndex] );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, 0, 0, nullptr,
	    0, nullptr, 1, &startRenderImageMemoryBarrier );

	// Dynamic rendering
	VkImageView currentImageView = m_swapchainImageViews[swapchainImageIndex];

	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkClearValue depthClear = {};
	depthClear.depthStencil.depth = 1.0f;

	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( currentImageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	colorAttachmentInfo.clearValue = colorClear;

	VkRenderingAttachmentInfo depthAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_depthImageView, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL );
	depthAttachmentInfo.clearValue = depthClear;

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( &colorAttachmentInfo, &depthAttachmentInfo, windowExtent );

	// Draw scene
	vkCmdBeginRendering( cmd, &renderInfo );

	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		// m_triangle->Render( m_camera, cmd, m_frameNumber );

		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			entity->Render( cmd, CalculateViewProjMatrix() );
		}
	} );

	vkCmdEndRendering( cmd );

#ifdef _IMGUI
	// Draw UI
	VkRenderingAttachmentInfo uiAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( currentImageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	uiAttachmentInfo.loadOp = VK_ATTACHMENT_LOAD_OP_LOAD; // Preserve existing color data (3d scene)

	VkRenderingInfo imguiRenderInfo = VKInit::RenderingInfo( &uiAttachmentInfo, nullptr, windowExtent );

	vkCmdBeginRendering( cmd, &imguiRenderInfo );
	ImGui_ImplVulkan_RenderDrawData( ImGui::GetDrawData(), cmd );
	vkCmdEndRendering( cmd );
#endif

	//
	// We want to present the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_PRESENT_SRC_KHR before presenting
	//
	VkImageMemoryBarrier endRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier(
	    VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_PRESENT_SRC_KHR, m_swapchainImages[swapchainImageIndex] );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT, 0, 0,
	    nullptr, 0, nullptr, 1, &endRenderImageMemoryBarrier );

	VK_CHECK( vkEndCommandBuffer( cmd ) );

	// Submit
	VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	VkSubmitInfo submit = VKInit::SubmitInfo( &cmd );

	submit.pWaitDstStageMask = &waitStage;

	submit.waitSemaphoreCount = 1;
	submit.pWaitSemaphores = &m_presentSemaphore;

	submit.signalSemaphoreCount = 1;
	submit.pSignalSemaphores = &m_renderSemaphore;

	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, m_renderFence ) );

	// Present
	VkPresentInfoKHR presentInfo = VKInit::PresentInfo( &m_swapchain, &m_renderSemaphore, &swapchainImageIndex );

	// We COULD have minimized the window between start and end.. so check again
	windowExtent = GetWindowExtent();

	if ( windowExtent.width < 1 || windowExtent.height < 1 )
	{
		// Do not render if we can't render to anything..
		return;
	}

	VK_CHECK( vkQueuePresentKHR( m_graphicsQueue, &presentInfo ) );

	m_frameNumber++;
}

void RenderManager::Run()
{
	bool bQuit = false;

	g_hostManager->FireEvent( "Event.Game.Load" );

	while ( !bQuit )
	{
		bQuit = m_window->Update();

		auto start = std::chrono::steady_clock::now();

		g_physicsManager->Update();

#ifdef _IMGUI
		ImGui_ImplVulkan_NewFrame();
		ImGui_ImplSDL2_NewFrame( m_window->GetSDLWindow() );
		ImGui::NewFrame();

		Editor::Draw();
		g_hostManager->DrawEditor();
#endif

		g_hostManager->Render();

		Render();

		auto end = std::chrono::steady_clock::now();
		std::chrono::duration<float> frameTime = end - start;

		g_frameTime = frameTime.count() * timescale.GetValue();
		g_curTime += g_frameTime * timescale.GetValue();
	}
}

void RenderManager::ImmediateSubmit( std::function<void( VkCommandBuffer cmd )>&& function )
{
	VkCommandBuffer cmd = m_uploadContext.commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo( VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT );
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	function( cmd );

	VK_CHECK( vkEndCommandBuffer( cmd ) );

	VkSubmitInfo submit = VKInit::SubmitInfo( &cmd );
	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, m_uploadContext.uploadFence ) );

	vkWaitForFences( m_device, 1, &m_uploadContext.uploadFence, true, 9999999999 );
	vkResetFences( m_device, 1, &m_uploadContext.uploadFence );

	vkResetCommandPool( m_device, m_uploadContext.commandPool, 0 );
}

AllocatedBuffer RenderManager::CreateBuffer( size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage )
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = allocationSize;
	bufferInfo.usage = usage;

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = memoryUsage;

	AllocatedBuffer buffer;
	VK_CHECK( vmaCreateBuffer( m_allocator, &bufferInfo, &allocInfo, &buffer.buffer, &buffer.allocation, nullptr ) );
	return buffer;
}

void RenderManager::CalculateCameraMatrices( glm::mat4& viewMatrix, glm::mat4& projMatrix )
{
	auto extent = GetWindowExtent();
	float aspect = ( float )extent.width / ( float )extent.height;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( g_cameraFov ), aspect, g_cameraZNear, g_cameraZFar );
}

glm::mat4 RenderManager::CalculateWorldToScreenMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	CalculateCameraMatrices( viewMatrix, projMatrix );

	return projMatrix * viewMatrix;
}

glm::mat4 RenderManager::CalculateViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	CalculateCameraMatrices( viewMatrix, projMatrix );

	return projMatrix * viewMatrix;
}
