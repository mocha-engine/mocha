#include "rendermanager.h"

//
//
//
#include <baseentity.h>
#include <cvarmanager.h>
#include <defs.h>
#include <edict.h>
#include <fontawesome.h>
#include <gamesettings.h>
#include <globalvars.h>
#include <hostmanager.h>
#include <mesh.h>
#include <modelentity.h>
#include <physicsmanager.h>
#include <shadercompiler.h>
#include <vk_types.h>
#include <vkinit.h>

//
//
//
#include <algorithm>
#include <fstream>
#include <iostream>
#include <memory>
#include <window.h>

//
//
//
#include <VkBootstrap.h>
#include <glm/ext.hpp>
#include <spdlog/spdlog.h>
#include <volk.h>

#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>

#ifdef _IMGUI
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <imgui.h>
#include <implot.h>
#endif

FloatCVar timescale( "timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
FloatCVar maxFramerate( "fps_max", 144.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

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
	volkInitialize();

	vkb::InstanceBuilder builder;

	auto ret = builder.set_app_name( GameSettings::Get()->name.c_str() )
	               .set_engine_name( ENGINE_NAME )
	               .request_validation_layers( true )
	               .require_api_version( 1, 3, 0 )
	               .use_default_debug_messenger()
	               .enable_extension( VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME )
	               .build();

	vkb::Instance vkbInstance = ret.value();

	m_instance = vkbInstance.instance;
	m_debugMessenger = vkbInstance.debug_messenger;

	volkLoadInstance( m_instance );

	m_surface = m_window->CreateSurface( m_instance );

	//
	// Set up physical device selection properties
	//
	vkb::PhysicalDeviceSelector selector( vkbInstance );
	// Minimum vulkan version, set target surface
	selector = selector.set_minimum_version( 1, 3 );
	selector = selector.set_surface( m_surface );

	if ( EngineFeatures::Raytracing )
	{
		//
		// Set required extensions
		//
		selector = selector.add_required_extension( VK_KHR_SPIRV_1_4_EXTENSION_NAME );
		selector = selector.add_required_extension( VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME );
		selector = selector.add_required_extension( VK_KHR_ACCELERATION_STRUCTURE_EXTENSION_NAME );
		selector = selector.add_required_extension( VK_KHR_RAY_QUERY_EXTENSION_NAME );
	}

	//
	// Set required VK1.0 features
	//
	VkPhysicalDeviceFeatures requiredFeatures = {};
	selector = selector.set_required_features( requiredFeatures );

	//
	// Set required VK1.1 features
	//
	VkPhysicalDeviceVulkan11Features requiredFeatures11 = {};
	requiredFeatures11.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_1_FEATURES;
	requiredFeatures11.pNext = nullptr;
	selector = selector.set_required_features_11( requiredFeatures11 );

	//
	// Set required VK1.2 features
	//
	VkPhysicalDeviceVulkan12Features requiredFeatures12 = {};
	requiredFeatures12.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_2_FEATURES;
	requiredFeatures12.pNext = nullptr;
	requiredFeatures12.descriptorIndexing = VK_TRUE;
	requiredFeatures12.bufferDeviceAddress = VK_TRUE;
	selector = selector.set_required_features_12( requiredFeatures12 );

	//
	// Set required VK1.3 features
	//
	VkPhysicalDeviceVulkan13Features requiredFeatures13 = {};
	requiredFeatures13.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_3_FEATURES;
	requiredFeatures13.pNext = nullptr;
	requiredFeatures13.dynamicRendering = VK_TRUE;
	selector = selector.set_required_features_13( requiredFeatures13 );

	//
	// Finalize and select a physical device
	//
	auto physicalDeviceReturn = selector.select();

	if ( !physicalDeviceReturn )
	{
		auto error = physicalDeviceReturn.full_error();

		std::string errorStr = "Couldn't find valid physical device: " + error.type.message();
		ERRORMESSAGE( errorStr );

		// Exit
		exit( error.type.value() );
	}

	vkb::PhysicalDevice physicalDevice = physicalDeviceReturn.value();
	vkb::DeviceBuilder deviceBuilder( physicalDevice );

	if ( EngineFeatures::Raytracing )
	{
		VkPhysicalDeviceAccelerationStructureFeaturesKHR accelFeature = {};
		accelFeature.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_ACCELERATION_STRUCTURE_FEATURES_KHR;
		accelFeature.accelerationStructure = true;

		VkPhysicalDeviceRayQueryFeaturesKHR rayQueryFeature = {};
		rayQueryFeature.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_QUERY_FEATURES_KHR;
		rayQueryFeature.rayQuery = true;

		deviceBuilder = deviceBuilder.add_pNext( &accelFeature ).add_pNext( &rayQueryFeature );
	}

	vkb::Device vkbDevice = deviceBuilder.build().value();

	m_device = vkbDevice.device;
	m_chosenGPU = vkbDevice.physical_device;

	m_graphicsQueue = vkbDevice.get_queue( vkb::QueueType::graphics ).value();
	m_graphicsQueueFamily = vkbDevice.get_queue_index( vkb::QueueType::graphics ).value();

	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.physicalDevice = m_chosenGPU;
	allocatorInfo.device = m_device;
	allocatorInfo.instance = m_instance;

	VmaVulkanFunctions allocatorFuncs = {};
	allocatorFuncs.vkGetInstanceProcAddr = vkGetInstanceProcAddr;
	allocatorFuncs.vkGetDeviceProcAddr = vkGetDeviceProcAddr;
	allocatorInfo.pVulkanFunctions = &allocatorFuncs;
	allocatorInfo.flags = VMA_ALLOCATOR_CREATE_BUFFER_DEVICE_ADDRESS_BIT;

	{
		std::vector<VkExtensionProperties> extensions;
		VkResult rs;

		uint32_t propertyCount;
		rs = vkEnumerateDeviceExtensionProperties( m_chosenGPU, nullptr, &propertyCount, nullptr );
		extensions.resize( propertyCount );
		rs = vkEnumerateDeviceExtensionProperties( m_chosenGPU, nullptr, &propertyCount, extensions.data() );

		spdlog::info( "Supported extensions ({}):", extensions.size() );
		for ( auto& ext : extensions )
		{
			spdlog::info( "\t{} {}", ext.extensionName, ext.specVersion );
		}
		spdlog::info( "=== END ===" );
	}

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
	VkExtent2D windowExtent = { ( uint32_t )windowWidth, ( uint32_t )windowHeight };

	return windowExtent;
}

VkExtent2D RenderManager::GetDesktopSize()
{
	int desktopWidth, desktopHeight;
	m_window->GetDesktopSize( &desktopWidth, &desktopHeight );
	VkExtent2D desktopExtent = { ( uint32_t )desktopWidth, ( uint32_t )desktopHeight };

	return desktopExtent;
}

void RenderManager::CreateSwapchain( VkExtent2D size )
{
	vkb::SwapchainBuilder swapchainBuilder( m_chosenGPU, m_device, m_surface );

	vkb::Swapchain vkbSwapchain = swapchainBuilder.set_old_swapchain( m_swapchain )
	                                  .set_desired_format( { VK_FORMAT_R8G8B8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	                                  .set_desired_present_mode( VK_PRESENT_MODE_MAILBOX_KHR )
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
	ImPlot::CreateContext();

	auto& io = ImGui::GetIO();
	m_mainFont = io.Fonts->AddFontFromFileTTF( "C:\\Windows\\Fonts\\segoeui.ttf", 16.0f );

	ImFontConfig iconConfig = {};
	iconConfig.MergeMode = 1;
	iconConfig.GlyphMinAdvanceX = 16.0f;

	ImWchar iconRanges[] = { ICON_MIN_FA, ICON_MAX_FA, 0 };

	io.Fonts->AddFontFromFileTTF( "content/core/fonts/fa-solid-900.ttf", 12.0f, &iconConfig, iconRanges );
	io.Fonts->AddFontFromFileTTF( "content/core/fonts/fa-regular-400.ttf", 12.0f, &iconConfig, iconRanges );

	m_monospaceFont = io.Fonts->AddFontFromFileTTF( "C:\\Windows\\Fonts\\CascadiaCode.ttf", 13.0f );

	io.Fonts->Build();

	io.ConfigFlags |= ImGuiConfigFlags_DockingEnable | ImGuiConfigFlags_ViewportsEnable;
	io.ConfigViewportsNoDecoration = false;
	io.ConfigViewportsNoAutoMerge = true;
	io.ConfigDockingWithShift = true;

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

	ImGui_ImplVulkan_LoadFunctions(
	    []( const char* function_name, void* user_data ) {
		    return vkGetInstanceProcAddr( ( VkInstance )user_data, function_name );
	    },
	    m_instance );
	ImGui_ImplVulkan_Init( &init_info, nullptr );
	ImmediateSubmit( [&]( VkCommandBuffer cmd ) { ImGui_ImplVulkan_CreateFontsTexture( cmd ); } );
	ImGui_ImplVulkan_DestroyFontUploadObjects();

	auto& style = ImGui::GetStyle();
	style.WindowPadding = { 8.00f, 8.00f };
	style.FramePadding = { 12.00f, 6.00f };
	style.CellPadding = { 4.00f, 4.00f };
	style.ItemSpacing = { 4.00f, 4.00f };
	style.ItemInnerSpacing = { 2.00f, 2.00f };
	style.TouchExtraPadding = { 0.00f, 0.00f };
	style.IndentSpacing = 25;
	style.ScrollbarSize = 12;
	style.GrabMinSize = 12;
	style.WindowBorderSize = 1;
	style.ChildBorderSize = 0;
	style.PopupBorderSize = 0;
	style.FrameBorderSize = 0;
	style.TabBorderSize = 0;
	style.WindowRounding = 6;
	style.ChildRounding = 4;
	style.FrameRounding = 3;
	style.PopupRounding = 4;
	style.ScrollbarRounding = 9;
	style.GrabRounding = 3;
	style.LogSliderDeadzone = 4;
	style.TabRounding = 4;
	style.WindowTitleAlign = { 0.5f, 0.5f };
	style.WindowMenuButtonPosition = ImGuiDir_None;
	style.AntiAliasedLinesUseTex = false;

	auto& colors = style.Colors;
	colors[ImGuiCol_Text] = { 1.00f, 1.00f, 1.00f, 1.00f };
	colors[ImGuiCol_TextDisabled] = { 0.50f, 0.50f, 0.50f, 1.00f };
	colors[ImGuiCol_WindowBg] = { 0.17f, 0.17f, 0.18f, 1.00f };
	colors[ImGuiCol_ChildBg] = { 0.10f, 0.11f, 0.11f, 1.00f };
	colors[ImGuiCol_PopupBg] = { 0.24f, 0.24f, 0.25f, 1.00f };
	colors[ImGuiCol_Border] = { 0.00f, 0.00f, 0.00f, 0.5f };
	colors[ImGuiCol_BorderShadow] = { 0.00f, 0.00f, 0.00f, 0.24f };
	colors[ImGuiCol_FrameBg] = { 0.10f, 0.11f, 0.11f, 1.00f };
	colors[ImGuiCol_FrameBgHovered] = { 0.19f, 0.19f, 0.19f, 0.54f };
	colors[ImGuiCol_FrameBgActive] = { 0.20f, 0.22f, 0.23f, 1.00f };
	colors[ImGuiCol_TitleBg] = { 0.0f, 0.0f, 0.0f, 1.00f };
	colors[ImGuiCol_TitleBgActive] = { 0.00f, 0.00f, 0.00f, 1.00f };
	colors[ImGuiCol_TitleBgCollapsed] = { 0.00f, 0.00f, 0.00f, 1.00f };
	colors[ImGuiCol_MenuBarBg] = { 0.14f, 0.14f, 0.14f, 1.00f };
	colors[ImGuiCol_ScrollbarBg] = { 0.05f, 0.05f, 0.05f, 0.54f };
	colors[ImGuiCol_ScrollbarGrab] = { 0.34f, 0.34f, 0.34f, 0.54f };
	colors[ImGuiCol_ScrollbarGrabHovered] = { 0.40f, 0.40f, 0.40f, 0.54f };
	colors[ImGuiCol_ScrollbarGrabActive] = { 0.56f, 0.56f, 0.56f, 0.54f };
	colors[ImGuiCol_CheckMark] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_SliderGrab] = { 0.34f, 0.34f, 0.34f, 0.54f };
	colors[ImGuiCol_SliderGrabActive] = { 0.56f, 0.56f, 0.56f, 0.54f };
	colors[ImGuiCol_Button] = { 0.24f, 0.24f, 0.25f, 1.00f };
	colors[ImGuiCol_ButtonHovered] = { 0.19f, 0.19f, 0.19f, 0.54f };
	colors[ImGuiCol_ButtonActive] = { 0.20f, 0.22f, 0.23f, 1.00f };
	colors[ImGuiCol_Header] = { 0.00f, 0.00f, 0.00f, 0.52f };
	colors[ImGuiCol_HeaderHovered] = { 0.00f, 0.00f, 0.00f, 0.36f };
	colors[ImGuiCol_HeaderActive] = { 0.20f, 0.22f, 0.23f, 0.33f };
	colors[ImGuiCol_Separator] = { 0.0f, 0.0f, 0.0f, 1.0f };
	colors[ImGuiCol_SeparatorHovered] = { 0.44f, 0.44f, 0.44f, 0.29f };
	colors[ImGuiCol_SeparatorActive] = { 0.40f, 0.44f, 0.47f, 1.00f };
	colors[ImGuiCol_ResizeGrip] = { 0.28f, 0.28f, 0.28f, 0.29f };
	colors[ImGuiCol_ResizeGripHovered] = { 0.44f, 0.44f, 0.44f, 0.29f };
	colors[ImGuiCol_ResizeGripActive] = { 0.40f, 0.44f, 0.47f, 1.00f };
	colors[ImGuiCol_Tab] = { 0.08f, 0.08f, 0.09f, 1.00f };
	colors[ImGuiCol_TabHovered] = { 0.14f, 0.14f, 0.14f, 1.00f };
	colors[ImGuiCol_TabActive] = { 0.17f, 0.17f, 0.18f, 1.00f };
	colors[ImGuiCol_TabUnfocused] = { 0.08f, 0.08f, 0.09f, 1.00f };
	colors[ImGuiCol_TabUnfocusedActive] = { 0.14f, 0.14f, 0.14f, 1.00f };
	colors[ImGuiCol_DockingPreview] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_DockingEmptyBg] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_PlotLines] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_PlotLinesHovered] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_PlotHistogram] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_PlotHistogramHovered] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_TableHeaderBg] = { 0.00f, 0.00f, 0.00f, 0.52f };
	colors[ImGuiCol_TableBorderStrong] = { 0.00f, 0.00f, 0.00f, 0.52f };
	colors[ImGuiCol_TableBorderLight] = { 0.28f, 0.28f, 0.28f, 0.29f };
	colors[ImGuiCol_TableRowBg] = { 0.00f, 0.00f, 0.00f, 0.00f };
	colors[ImGuiCol_TableRowBgAlt] = { 1.00f, 1.00f, 1.00f, 0.06f };
	colors[ImGuiCol_TextSelectedBg] = { 0.20f, 0.22f, 0.23f, 1.00f };
	colors[ImGuiCol_DragDropTarget] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_NavHighlight] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_NavWindowingHighlight] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_NavWindowingDimBg] = { 0.33f, 0.67f, 0.86f, 1.00f };
	colors[ImGuiCol_ModalWindowDimBg] = { 0.33f, 0.67f, 0.86f, 1.00f };
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

void RenderManager::InitRayTracing()
{
	spdlog::info( "Ray tracing enabled" );

	m_rtProperties = {};
	m_rtProperties.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_PIPELINE_PROPERTIES_KHR;
	m_rtProperties.pNext = nullptr;

	// Requesting ray tracing properties
	VkPhysicalDeviceProperties2 prop2 = {};
	prop2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PROPERTIES_2;
	prop2.pNext = &m_rtProperties;

	vkGetPhysicalDeviceProperties2( m_chosenGPU, &prop2 );

	vkGetPhysicalDeviceMemoryProperties( m_chosenGPU, &m_memoryProperties );
}

BlasInput RenderManager::ModelToVkGeometry( Model& model )
{
	BlasInput input = {};

	for ( const auto& mesh : model.GetMeshes() )
	{
		// BLAS builder requires raw device addresses.
		VkBufferDeviceAddressInfo bufferInfo = {};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO;
		bufferInfo.pNext = nullptr;

		bufferInfo.buffer = mesh.vertexBuffer.buffer;
		VkDeviceAddress vertexAddress = vkGetBufferDeviceAddress( m_device, &bufferInfo );

		bufferInfo.buffer = mesh.indexBuffer.buffer;
		VkDeviceAddress indexAddress = vkGetBufferDeviceAddress( m_device, &bufferInfo );

		uint32_t maxPrimitiveCount = mesh.indices.count / 3;

		// Describe buffer as array of VertexObj.
		VkAccelerationStructureGeometryTrianglesDataKHR triangles{
		    VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_TRIANGLES_DATA_KHR };

		triangles.vertexFormat = VK_FORMAT_R32G32B32_SFLOAT; // vec3 vertex position data.
		triangles.vertexData.deviceAddress = vertexAddress;
		triangles.vertexStride = 17 * sizeof( float );

		triangles.indexType = VK_INDEX_TYPE_UINT32;
		triangles.indexData.deviceAddress = indexAddress;

		triangles.maxVertex = mesh.vertices.count;

		// Identify the above data as containing opaque triangles.
		VkAccelerationStructureGeometryKHR asGeom{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR };
		asGeom.geometryType = VK_GEOMETRY_TYPE_TRIANGLES_KHR;
		asGeom.flags = VK_GEOMETRY_OPAQUE_BIT_KHR;
		asGeom.geometry.triangles = triangles;

		// The entire array will be used to build the BLAS.
		VkAccelerationStructureBuildRangeInfoKHR offset;
		offset.firstVertex = 0;
		offset.primitiveCount = maxPrimitiveCount;
		offset.primitiveOffset = 0;
		offset.transformOffset = 0;

		// Our blas is made from one geometry per mesh (many meshes : one model)
		input.asGeometry.emplace_back( asGeom );
		input.asBuildOffsetInfo.emplace_back( offset );
	}

	return input;
}

void RenderManager::CreateBottomLevelAS()
{
	std::vector<BlasInput> allBlas;

	g_entityDictionary->For( [&]( uint32_t handle, std::shared_ptr<BaseEntity> entity ) {
		auto modelEntity = std::dynamic_pointer_cast<ModelEntity>( entity );

		if ( modelEntity == nullptr )
			return;

		if ( handle > 0 )
			return;

		auto model = modelEntity->GetModel();
		auto input = ModelToVkGeometry( model );

		allBlas.emplace_back( input );
	} );

	// Build blas
	VkBuildAccelerationStructureFlagsKHR flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
	uint32_t nbBlas = static_cast<uint32_t>( allBlas.size() );
	uint32_t nbCompactions = 0;
	VkDeviceSize asTotalSize = 0;
	VkDeviceSize maxScratchSize = 0;

	std::vector<BuildAccelerationStructure> buildAs( nbBlas );

	for ( uint32_t idx = 0; idx < nbBlas; ++idx )
	{
		// Filling partially the VkAccelerationStructureBuildGeometryInfoKHR for querying the build sizes.
		// Other information will be filled in the createBlas (see #2)
		buildAs[idx].buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
		buildAs[idx].buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
		buildAs[idx].buildInfo.mode = VK_BUILD_ACCELERATION_STRUCTURE_MODE_BUILD_KHR;
		buildAs[idx].buildInfo.flags = allBlas[idx].flags | flags;
		buildAs[idx].buildInfo.geometryCount = static_cast<uint32_t>( allBlas[idx].asGeometry.size() );
		buildAs[idx].buildInfo.pGeometries = allBlas[idx].asGeometry.data();

		// Build range information
		buildAs[idx].rangeInfo = allBlas[idx].asBuildOffsetInfo.data();

		buildAs[idx].sizeInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_SIZES_INFO_KHR;

		// Finding sizes to create acceleration structures and scratch
		std::vector<uint32_t> maxPrimCount( allBlas[idx].asBuildOffsetInfo.size() );
		for ( auto tt = 0; tt < allBlas[idx].asBuildOffsetInfo.size(); tt++ )
			maxPrimCount[tt] = allBlas[idx].asBuildOffsetInfo[tt].primitiveCount; // Number of primitives/triangles
		vkGetAccelerationStructureBuildSizesKHR( m_device, VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR,
		    &buildAs[idx].buildInfo, maxPrimCount.data(), &buildAs[idx].sizeInfo );

		// Extra info
		asTotalSize += buildAs[idx].sizeInfo.accelerationStructureSize;
		maxScratchSize = std::max( maxScratchSize, buildAs[idx].sizeInfo.buildScratchSize );
		nbCompactions += ( buildAs[idx].buildInfo.flags & VK_BUILD_ACCELERATION_STRUCTURE_ALLOW_COMPACTION_BIT_KHR ) == 1;
	}

	// Allocate scratch buffer
	AllocatedBuffer scratchBuffer = CreateBuffer(
	    maxScratchSize, VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT, VMA_MEMORY_USAGE_AUTO );

	VkBufferDeviceAddressInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO;
	bufferInfo.pNext = nullptr;
	bufferInfo.buffer = scratchBuffer.buffer;

	VkDeviceAddress scratchAddress = vkGetBufferDeviceAddress( m_device, &bufferInfo );

	// Allocate a query pool for storing the needed size for every BLAS compaction.
	VkQueryPool queryPool = { VK_NULL_HANDLE };
	if ( nbCompactions > 0 )
	{
		assert( nbCompactions == nbBlas );
		VkQueryPoolCreateInfo qpci;
		qpci.sType = VK_STRUCTURE_TYPE_QUERY_POOL_CREATE_INFO;
		qpci.pNext = nullptr;

		qpci.queryCount = nbBlas;
		qpci.queryType = VK_QUERY_TYPE_ACCELERATION_STRUCTURE_COMPACTED_SIZE_KHR;

		vkCreateQueryPool( m_device, &qpci, nullptr, &queryPool );
	}

	// Batching creation / compaction of BLAS to allow staying in restricted amount of memory
	std::vector<uint32_t> indices; // Indices of the BLAS to create
	VkDeviceSize batchSize{ 0 };
	VkDeviceSize batchLimit{ 256'000'000 }; // 256 MB
	for ( uint32_t idx = 0; idx < nbBlas; idx++ )
	{
		indices.push_back( idx );
		batchSize += buildAs[idx].sizeInfo.accelerationStructureSize;
		// Over the limit or last BLAS element
		if ( batchSize >= batchLimit || idx == nbBlas - 1 )
		{
			ImmediateSubmit(
			    [&]( VkCommandBuffer cmdBuf ) { CmdCreateBlas( cmdBuf, indices, buildAs, scratchAddress, queryPool ); } );

			if ( queryPool )
			{
				ImmediateSubmit( [&]( VkCommandBuffer cmdBuf ) { CmdCompactBlas( cmdBuf, indices, buildAs, queryPool ); } );
				// Destroy the non-compacted version
				// destroyNonCompacted( indices, buildAs );
			}
			// Reset

			batchSize = 0;
			indices.clear();
		}
	}

	// Keeping all the created acceleration structures
	for ( auto& b : buildAs )
	{
		m_blas.emplace_back( b.as );
	}

	// Clean up
	vkDestroyQueryPool( m_device, queryPool, nullptr );

	spdlog::info( "Built bottom-level AS with {} inputs", allBlas.size() );
}

void RenderManager::CreateTopLevelAS()
{
	std::vector<VkAccelerationStructureInstanceKHR> instances;

	g_entityDictionary->For( [&]( uint32_t handle, std::shared_ptr<BaseEntity> entity ) {
		auto modelEntity = std::dynamic_pointer_cast<ModelEntity>( entity );

		if ( modelEntity == nullptr )
			return;

		if ( handle > 0 )
			return;

		VkAccelerationStructureInstanceKHR rayInst = {};

		// clang-format off
		glm::mat4x4 inputMatrix = modelEntity->GetTransform().GetModelMatrix();
		VkTransformMatrixKHR transformMatrix = {
            inputMatrix[0][0], inputMatrix[1][0], inputMatrix[2][0], inputMatrix[3][0],
            inputMatrix[0][1], inputMatrix[1][1], inputMatrix[2][1], inputMatrix[3][1],
            inputMatrix[0][2], inputMatrix[1][2], inputMatrix[2][2], inputMatrix[3][2],
		};
		// clang-format on

		rayInst.transform = transformMatrix;
		rayInst.instanceCustomIndex = handle;
		rayInst.accelerationStructureReference = GetBlasDeviceAddress( handle );
		rayInst.flags = VK_GEOMETRY_INSTANCE_TRIANGLE_FACING_CULL_DISABLE_BIT_KHR;
		rayInst.mask = 0xFF;
		rayInst.instanceShaderBindingTableRecordOffset = 0;

		instances.emplace_back( rayInst );
	} );

	assert( m_tlas.accel == VK_NULL_HANDLE ); // Cannot call buildTlas twice except to update.
	uint32_t countInstance = static_cast<uint32_t>( instances.size() );

	ImmediateSubmit( [&]( VkCommandBuffer cmdBuf ) {
		// Create a buffer holding the actual instance data (matrices++) for use by the AS builder
		AllocatedBuffer instancesBuffer; // Buffer of instances containing the matrices and BLAS ids

		instancesBuffer = CreateBuffer( sizeof( VkAccelerationStructureInstanceKHR ) * instances.size(),
		    VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT | VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_BUILD_INPUT_READ_ONLY_BIT_KHR,
		    VMA_MEMORY_USAGE_AUTO );

		void* mappedData;
		vmaMapMemory( g_renderManager->m_allocator, instancesBuffer.allocation, &mappedData );
		memcpy( mappedData, instances.data(),
		    static_cast<size_t>( sizeof( VkAccelerationStructureInstanceKHR ) * instances.size() ) );
		vmaUnmapMemory( g_renderManager->m_allocator, instancesBuffer.allocation );

		VkBufferDeviceAddressInfo bufferInfo{ VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO, nullptr, instancesBuffer.buffer };
		VkDeviceAddress instBufferAddr = vkGetBufferDeviceAddress( m_device, &bufferInfo );

		// Make sure the copy of the instance buffer are copied before triggering the acceleration structure build
		VkMemoryBarrier barrier{ VK_STRUCTURE_TYPE_MEMORY_BARRIER };
		barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_ACCELERATION_STRUCTURE_WRITE_BIT_KHR;
		vkCmdPipelineBarrier( cmdBuf, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR, 0,
		    1, &barrier, 0, nullptr, 0, nullptr );

		// Creating the TLAS
		AllocatedBuffer scratchBuffer;
		CmdCreateTlas( cmdBuf, countInstance, instBufferAddr, scratchBuffer,
		    VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR, false );
	} );

	spdlog::info( "Built top-level AS with {} instances", instances.size() );
}

VkDeviceAddress RenderManager::GetBlasDeviceAddress( uint32_t handle )
{
	assert( size_t( handle ) < m_blas.size() );
	VkAccelerationStructureDeviceAddressInfoKHR addressInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_DEVICE_ADDRESS_INFO_KHR };
	addressInfo.accelerationStructure = m_blas[handle].accel;
	return vkGetAccelerationStructureDeviceAddressKHR( m_device, &addressInfo );
}

uint32_t RenderManager::GetMemoryType( uint32_t typeBits, VkMemoryPropertyFlags properties, VkBool32* memTypeFound ) const
{
	for ( uint32_t i = 0; i < m_memoryProperties.memoryTypeCount; i++ )
	{
		if ( ( typeBits & 1 ) == 1 )
		{
			if ( ( m_memoryProperties.memoryTypes[i].propertyFlags & properties ) == properties )
			{
				if ( memTypeFound )
				{
					*memTypeFound = true;
				}
				return i;
			}
		}
		typeBits >>= 1;
	}

	if ( memTypeFound )
	{
		*memTypeFound = false;
		return 0;
	}
	else
	{
		throw std::runtime_error( "Could not find a matching memory type" );
	}
}

AllocatedAccel RenderManager::CreateAcceleration( VkAccelerationStructureCreateInfoKHR& createInfo )
{
	AllocatedAccel accel = {};

	accel.buffer = CreateBuffer( createInfo.size,
	    VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_STORAGE_BIT_KHR | VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT,
	    VMA_MEMORY_USAGE_AUTO );

	createInfo.buffer = accel.buffer.buffer;

	vkCreateAccelerationStructureKHR( m_device, &createInfo, nullptr, &accel.accel );

	return accel;
}

void RenderManager::CmdCreateBlas( VkCommandBuffer cmdBuf, std::vector<uint32_t> indices,
    std::vector<BuildAccelerationStructure>& buildAs, VkDeviceAddress scratchAddress, VkQueryPool queryPool )
{
	if ( queryPool )
		vkResetQueryPool( m_device, queryPool, 0, static_cast<uint32_t>( indices.size() ) );

	uint32_t queryCount = 0;

	for ( const auto& idx : indices )
	{
		// Actual allocation of buffer and acceleration structure.
		VkAccelerationStructureCreateInfoKHR createInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR };
		createInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
		createInfo.size = buildAs[idx].sizeInfo.accelerationStructureSize; // Will be used to allocate memory.
		buildAs[idx].as = CreateAcceleration( createInfo );

		// BuildInfo #2 part
		buildAs[idx].buildInfo.dstAccelerationStructure = buildAs[idx].as.accel; // Setting where the build lands
		buildAs[idx].buildInfo.scratchData.deviceAddress = scratchAddress;       // All build are using the same scratch buffer

		// Building the bottom-level-acceleration-structure
		vkCmdBuildAccelerationStructuresKHR( cmdBuf, 1, &buildAs[idx].buildInfo, &buildAs[idx].rangeInfo );

		// Since the scratch buffer is reused across builds, we need a barrier to ensure one build
		// is finished before starting the next one.
		VkMemoryBarrier barrier{ VK_STRUCTURE_TYPE_MEMORY_BARRIER };
		barrier.srcAccessMask = VK_ACCESS_ACCELERATION_STRUCTURE_WRITE_BIT_KHR;
		barrier.dstAccessMask = VK_ACCESS_ACCELERATION_STRUCTURE_READ_BIT_KHR;
		vkCmdPipelineBarrier( cmdBuf, VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR,
		    VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR, 0, 1, &barrier, 0, nullptr, 0, nullptr );

		if ( queryPool )
		{
			// Add a query to find the 'real' amount of memory needed, use for compaction
			vkCmdWriteAccelerationStructuresPropertiesKHR( cmdBuf, 1, &buildAs[idx].buildInfo.dstAccelerationStructure,
			    VK_QUERY_TYPE_ACCELERATION_STRUCTURE_COMPACTED_SIZE_KHR, queryPool, queryCount++ );
		}
	}
}
void RenderManager::CmdCompactBlas( VkCommandBuffer cmdBuf, std::vector<uint32_t> indices,
    std::vector<BuildAccelerationStructure>& buildAs, VkQueryPool queryPool )
{
	uint32_t queryCtn{ 0 };
	std::vector<AllocatedAccel> cleanupAS; // previous AS to destroy

	// Get the compacted size result back
	std::vector<VkDeviceSize> compactSizes( static_cast<uint32_t>( indices.size() ) );
	vkGetQueryPoolResults( m_device, queryPool, 0, ( uint32_t )compactSizes.size(),
	    compactSizes.size() * sizeof( VkDeviceSize ), compactSizes.data(), sizeof( VkDeviceSize ), VK_QUERY_RESULT_WAIT_BIT );

	for ( auto idx : indices )
	{
		buildAs[idx].cleanupAS = buildAs[idx].as;                                   // previous AS to destroy
		buildAs[idx].sizeInfo.accelerationStructureSize = compactSizes[queryCtn++]; // new reduced size

		// Creating a compact version of the AS
		VkAccelerationStructureCreateInfoKHR asCreateInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR };
		asCreateInfo.size = buildAs[idx].sizeInfo.accelerationStructureSize;
		asCreateInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
		buildAs[idx].as = CreateAcceleration( asCreateInfo );

		// Copy the original BLAS to a compact version
		VkCopyAccelerationStructureInfoKHR copyInfo{ VK_STRUCTURE_TYPE_COPY_ACCELERATION_STRUCTURE_INFO_KHR };
		copyInfo.src = buildAs[idx].buildInfo.dstAccelerationStructure;
		copyInfo.dst = buildAs[idx].as.accel;
		copyInfo.mode = VK_COPY_ACCELERATION_STRUCTURE_MODE_COMPACT_KHR;
		vkCmdCopyAccelerationStructureKHR( cmdBuf, &copyInfo );
	}
}
void RenderManager::CmdCreateTlas( VkCommandBuffer cmdBuf, uint32_t countInstance, VkDeviceAddress instBufferAddr,
    AllocatedBuffer& scratchBuffer, VkBuildAccelerationStructureFlagsKHR flags, bool update )
{
	// Wraps a device pointer to the above uploaded instances.
	VkAccelerationStructureGeometryInstancesDataKHR instancesVk{
	    VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_INSTANCES_DATA_KHR };
	instancesVk.data.deviceAddress = instBufferAddr;

	// Put the above into a VkAccelerationStructureGeometryKHR. We need to put the instances struct in a union and label it as
	// instance data.
	VkAccelerationStructureGeometryKHR topASGeometry{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR };
	topASGeometry.geometryType = VK_GEOMETRY_TYPE_INSTANCES_KHR;
	topASGeometry.geometry.instances = instancesVk;

	// Find sizes
	VkAccelerationStructureBuildGeometryInfoKHR buildInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR };
	buildInfo.flags = flags;
	buildInfo.geometryCount = 1;
	buildInfo.pGeometries = &topASGeometry;
	buildInfo.mode = update ? VK_BUILD_ACCELERATION_STRUCTURE_MODE_UPDATE_KHR : VK_BUILD_ACCELERATION_STRUCTURE_MODE_BUILD_KHR;
	buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
	buildInfo.srcAccelerationStructure = VK_NULL_HANDLE;

	VkAccelerationStructureBuildSizesInfoKHR sizeInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_SIZES_INFO_KHR };
	vkGetAccelerationStructureBuildSizesKHR(
	    m_device, VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR, &buildInfo, &countInstance, &sizeInfo );

	VkAccelerationStructureCreateInfoKHR createInfo{ VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR };
	createInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
	createInfo.size = sizeInfo.accelerationStructureSize;

	m_tlas = CreateAcceleration( createInfo );

	// Allocate the scratch memory
	scratchBuffer = CreateBuffer( sizeInfo.buildScratchSize,
	    VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT, VMA_MEMORY_USAGE_AUTO );

	VkBufferDeviceAddressInfo bufferInfo{ VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO, nullptr, scratchBuffer.buffer };
	VkDeviceAddress scratchAddress = vkGetBufferDeviceAddress( m_device, &bufferInfo );

	// Update build information
	buildInfo.srcAccelerationStructure = VK_NULL_HANDLE;
	buildInfo.dstAccelerationStructure = m_tlas.accel;
	buildInfo.scratchData.deviceAddress = scratchAddress;

	// Build Offsets info: n instances
	VkAccelerationStructureBuildRangeInfoKHR buildOffsetInfo{ countInstance, 0, 0, 0 };
	const VkAccelerationStructureBuildRangeInfoKHR* pBuildOffsetInfo = &buildOffsetInfo;

	// Build the TLAS
	vkCmdBuildAccelerationStructuresKHR( cmdBuf, 1, &buildInfo, &pBuildOffsetInfo );
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

	auto viewProjMatrix = CalculateViewProjMatrix();
	auto viewmodelViewProjMatrix = CalculateViewmodelViewProjMatrix();

	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( !renderEntity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) && !renderEntity->HasFlag( EntityFlags::ENTITY_UI ) )
				entity->Render( cmd, viewProjMatrix );
		}
	} );

	//
	// Render viewmodels
	//
	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( renderEntity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) )
				entity->Render( cmd, viewmodelViewProjMatrix );
		}
	} );

	//
	// Render UI last
	//
	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( renderEntity->HasFlag( EntityFlags::ENTITY_UI ) )
				entity->Render( cmd, viewProjMatrix );
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

	if ( EngineFeatures::Raytracing )
	{
		InitRayTracing();
		CreateBottomLevelAS();
		CreateTopLevelAS();
	}

	while ( !bQuit )
	{
		static auto gameStart = std::chrono::steady_clock::now();
		static float flFilteredTime = 0;
		static float flPreviousTime = 0;
		static float flFrameTime = 0;

		std::chrono::duration<float> timeSinceStart = std::chrono::steady_clock::now() - gameStart;
		float flCurrentTime = timeSinceStart.count();

		float dt = flCurrentTime - flPreviousTime;
		flPreviousTime = flCurrentTime;

		flFrameTime += dt;

		if ( flFrameTime < 0.0f )
			return;

		// How quick did we do last frame? Let's limit ourselves if (1.0f / g_frameTime) is more than maxFramerate
		float fps = 1.0f / flFrameTime;
		float maxFps = maxFramerate.GetValue();

		if ( maxFps > 0 && fps > maxFps )
		{
			flFilteredTime += g_frameTime;
			continue;
		}

		g_curTime = flCurrentTime;
		g_frameTime = flFrameTime;

		flFilteredTime = 0;
		flFrameTime = 0;

		bQuit = m_window->Update();

		g_physicsManager->Update();

#ifdef _IMGUI
		ImGui_ImplVulkan_NewFrame();
		ImGui_ImplSDL2_NewFrame( m_window->GetSDLWindow() );
		ImGui::NewFrame();
		ImGui::DockSpaceOverViewport( nullptr, ImGuiDockNodeFlags_PassthruCentralNode );
		g_hostManager->DrawEditor();
#endif

		g_hostManager->Render();

#ifdef _IMGUI
		ImGui::Render();

		if ( ImGui::GetIO().ConfigFlags & ImGuiConfigFlags_ViewportsEnable )
		{
			ImGui::UpdatePlatformWindows();
			ImGui::RenderPlatformWindowsDefault();
		}
#endif

		Render();
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

AllocatedBuffer RenderManager::CreateBuffer(
    size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage, VmaAllocationCreateFlagBits allocFlags )
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = allocationSize;
	bufferInfo.usage = usage;

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = memoryUsage;
	allocInfo.flags = allocFlags;

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

glm::mat4 RenderManager::CalculateViewmodelViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.width / ( float )extent.height;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( 60.0f ), aspect, g_cameraZNear, g_cameraZFar );

	return projMatrix * viewMatrix;
}

glm::mat4 RenderManager::CalculateViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.width / ( float )extent.height;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( g_cameraFov ), aspect, g_cameraZNear, g_cameraZFar );

	return projMatrix * viewMatrix;
}
