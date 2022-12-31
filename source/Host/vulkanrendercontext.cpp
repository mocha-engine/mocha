#include "vulkanrendercontext.h"

#include <volk.h>

// ----------------------------------------------------------------------------------------------------

void VulkanSwapchain::CreateMainSwapchain( VkDevice device, VkPhysicalDevice physicalDevice, VkSurfaceKHR surface, Size2D size )
{
	vkb::SwapchainBuilder swapchainBuilder( physicalDevice, device, surface );

	vkb::Swapchain vkbSwapchain = swapchainBuilder.set_old_swapchain( m_swapchain )
	                                  .set_desired_format( { VK_FORMAT_R8G8B8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	                                  .set_desired_present_mode( VK_PRESENT_MODE_MAILBOX_KHR )
	                                  .set_desired_extent( size.x, size.y )
	                                  .build()
	                                  .value();

	m_swapchain = vkbSwapchain.swapchain;
	m_images = vkbSwapchain.get_images().value();
	m_imageViews = vkbSwapchain.get_image_views().value();
	m_imageFormat = vkbSwapchain.image_format;
}

void VulkanSwapchain::CreateDepthTexture( VkDevice device, Size2D size )
{
	m_depthTexture = VulkanRenderTexture( device, size, RENDER_TEXTURE_DEPTH );
}

VulkanSwapchain::VulkanSwapchain( VkDevice device, VkPhysicalDevice physicalDevice, VkSurfaceKHR surface, Size2D size )
{
	CreateMainSwapchain( device, physicalDevice, surface, size );
	CreateDepthTexture( device, size );
}

// ----------------------------------------------------------------------------------------------------

VkImageUsageFlagBits VulkanRenderTexture::GetUsageFlagBits( RenderTextureType type )
{
	switch ( type )
	{
	case RENDER_TEXTURE_COLOR:
	case RENDER_TEXTURE_COLOR_OPAQUE:
		return VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
	case RENDER_TEXTURE_DEPTH:
		return VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;
	}

	assert( true && "Invalid render texture type" );
}

VkFormat VulkanRenderTexture::GetFormat( RenderTextureType type )
{
	switch ( type )
	{
	case RENDER_TEXTURE_COLOR:
		return VK_FORMAT_R8G8B8A8_UNORM;
	case RENDER_TEXTURE_COLOR_OPAQUE:
		return VK_FORMAT_R8G8B8_UNORM;
	case RENDER_TEXTURE_DEPTH:
		return VK_FORMAT_D32_SFLOAT;
	}

	assert( true && "Invalid render texture type" );
}

VkImageAspectFlags VulkanRenderTexture::GetAspectFlags( RenderTextureType type )
{
	switch ( type )
	{
	case RENDER_TEXTURE_COLOR:
	case RENDER_TEXTURE_COLOR_OPAQUE:
		return VK_IMAGE_ASPECT_COLOR_BIT;
	case RENDER_TEXTURE_DEPTH:
		return VK_IMAGE_ASPECT_DEPTH_BIT;
	}

	assert( true && "Invalid render texture type" );
}

VulkanRenderTexture::VulkanRenderTexture( VkDevice device, Size2D size, RenderTextureType type )
{
	VkExtent3D depthImageExtent = {
	    size.x,
	    size.y,
	    1,
	};

	format = GetFormat( type ); // Depth & stencil format

	VkImageCreateInfo imageInfo = VKInit::ImageCreateInfo( format, GetUsageFlagBits( type ), depthImageExtent, 1 );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	allocInfo.requiredFlags = VkMemoryPropertyFlags( VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT );

	vmaCreateImage( *g_allocator, &imageInfo, &allocInfo, &image, &allocation, nullptr );

	VkImageViewCreateInfo viewInfo = VKInit::ImageViewCreateInfo( format, image, GetAspectFlags( type ), 1 );
	VK_CHECK( vkCreateImageView( device, &viewInfo, nullptr, &imageView ) );
}

// ----------------------------------------------------------------------------------------------------

VulkanCommandContext::VulkanCommandContext( VkDevice device, uint32_t graphicsQueueFamily )
{
	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.pNext = nullptr;

	poolInfo.queueFamilyIndex = graphicsQueueFamily;
	poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

	VK_CHECK( vkCreateCommandPool( device, &poolInfo, nullptr, &commandPool ) );

	VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.pNext = nullptr;

	allocInfo.commandPool = commandPool;
	allocInfo.commandBufferCount = 1;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

	VK_CHECK( vkAllocateCommandBuffers( device, &allocInfo, &commandBuffer ) );

	VkFenceCreateInfo fenceInfo = {};
	fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceInfo.pNext = nullptr;
	fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

	VK_CHECK( vkCreateFence( device, &fenceInfo, nullptr, &fence ) );
	vkResetFences( device, 1, &fence );
}

// ----------------------------------------------------------------------------------------------------

VkSamplerCreateInfo VulkanSampler::GetCreateInfo( SamplerType samplerType )
{
	if ( samplerType == SAMPLER_TYPE_POINT )
		return VKInit::SamplerCreateInfo( VK_FILTER_NEAREST, VK_SAMPLER_ADDRESS_MODE_REPEAT, false );
	if ( samplerType == SAMPLER_TYPE_LINEAR )
		return VKInit::SamplerCreateInfo( VK_FILTER_LINEAR, VK_SAMPLER_ADDRESS_MODE_REPEAT, false );
	if ( samplerType == SAMPLER_TYPE_ANISOTROPIC )
		return VKInit::SamplerCreateInfo( VK_FILTER_LINEAR, VK_SAMPLER_ADDRESS_MODE_REPEAT, true );

	assert( true && "Invalid sampler type." );
}

VulkanSampler::VulkanSampler( VkDevice m_device, SamplerType samplerType )
{
	VkSamplerCreateInfo samplerInfo = GetCreateInfo( samplerType );
	VK_CHECK( vkCreateSampler( m_device, &samplerInfo, nullptr, &sampler ) );
}

// ----------------------------------------------------------------------------------------------------

VulkanBuffer::VulkanBuffer( VkDevice m_device, size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage,
    VmaAllocationCreateFlagBits allocFlags )
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = allocationSize;
	bufferInfo.usage = usage;

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = memoryUsage;
	allocInfo.flags = allocFlags;

	VK_CHECK( vmaCreateBuffer( *g_allocator, &bufferInfo, &allocInfo, &buffer, &allocation, nullptr ) );
}

// ----------------------------------------------------------------------------------------------------

// Create a Vulkan context, set up devices
vkb::Instance VulkanRenderContext::CreateInstanceAndSurface()
{
	vkb::InstanceBuilder builder;
	vkb::Instance vkbInstance;

	auto ret = builder.set_app_name( GameSettings::Get()->name.c_str() )
	               .set_engine_name( ENGINE_NAME )
	               .request_validation_layers( true )
	               .require_api_version( 1, 3, 0 )
	               .use_default_debug_messenger()
	               .enable_extension( VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME )
	               .build();

	vkbInstance = ret.value();

	m_instance = vkbInstance.instance;
	m_debugMessenger = vkbInstance.debug_messenger;

	volkLoadInstance( m_instance );

	m_surface = m_window->CreateSurface( m_instance );

	return vkbInstance;
}

void VulkanRenderContext::FinalizeAndCreateDevice( vkb::PhysicalDevice physicalDevice )
{
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

	// Save device properties for later
	VkPhysicalDeviceProperties deviceProperties = {};
	vkGetPhysicalDeviceProperties( m_chosenGPU, &m_deviceProperties );
}

vkb::PhysicalDevice VulkanRenderContext::CreatePhysicalDevice( vkb::Instance vkbInstance )
{
	//
	// Set up physical device selection properties
	//
	vkb::PhysicalDeviceSelector selector( vkbInstance );
	// Minimum vulkan version, set target surface
	selector = selector.set_minimum_version( 1, 3 );
	selector = selector.set_surface( m_surface );

	//
	// Set required extensions
	//
	if ( EngineFeatures::Raytracing )
	{
#define X( name ) selector = selector.add_required_extension( name );
		X( VK_KHR_SPIRV_1_4_EXTENSION_NAME );
		X( VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME );
		X( VK_KHR_ACCELERATION_STRUCTURE_EXTENSION_NAME );
		X( VK_KHR_RAY_QUERY_EXTENSION_NAME );
#undef X
	}

	//
	// Set required VK1.0 features
	//
	VkPhysicalDeviceFeatures requiredFeatures = {};
	requiredFeatures.samplerAnisotropy = VK_TRUE;
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

	return physicalDeviceReturn.value();
}

void VulkanRenderContext::CreateSwapchain()
{
	Size2D size = m_window->GetWindowSize();

	m_swapchain = VulkanSwapchain( m_device, m_chosenGPU, m_surface, size );
}

void VulkanRenderContext::CreateCommands()
{
	m_mainContext = VulkanCommandContext( m_device, m_graphicsQueueFamily );
	m_uploadContext = VulkanCommandContext( m_device, m_graphicsQueueFamily );
}

void VulkanRenderContext::CreateSyncStructures()
{
	VkSemaphoreCreateInfo semaphoreCreateInfo = {};
	semaphoreCreateInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreCreateInfo.pNext = nullptr;
	semaphoreCreateInfo.flags = 0;

	VK_CHECK( vkCreateSemaphore( m_device, &semaphoreCreateInfo, nullptr, &m_presentSemaphore ) );
	VK_CHECK( vkCreateSemaphore( m_device, &semaphoreCreateInfo, nullptr, &m_renderSemaphore ) );

	// Fences are handled by VulkanCommandContexts. Our main fence is m_mainContext.fence.
}

void VulkanRenderContext::CreateDescriptors()
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

void VulkanRenderContext::CreateSamplers()
{
	m_pointSampler = VulkanSampler( m_device, SAMPLER_TYPE_POINT );
	m_anisoSampler = VulkanSampler( m_device, SAMPLER_TYPE_ANISOTROPIC );
}

void VulkanRenderContext::CreateAllocator()
{
	VmaAllocatorCreateInfo allocatorInfo = {};
	allocatorInfo.physicalDevice = m_chosenGPU;
	allocatorInfo.device = m_device;
	allocatorInfo.instance = m_instance;

	VmaVulkanFunctions allocatorFuncs = {};
	allocatorFuncs.vkGetInstanceProcAddr = vkGetInstanceProcAddr;
	allocatorFuncs.vkGetDeviceProcAddr = vkGetDeviceProcAddr;
	allocatorInfo.pVulkanFunctions = &allocatorFuncs;
	allocatorInfo.flags = VMA_ALLOCATOR_CREATE_BUFFER_DEVICE_ADDRESS_BIT;

	vmaCreateAllocator( &allocatorInfo, &m_allocator );
}

RenderContextStatus VulkanRenderContext::Startup()
{
	ErrorIf( m_hasInitialized, RenderContextStatus::ALREADY_INITIALIZED );

	volkInitialize();

	//
	// Create vulkan objects
	//
	{
		// Initial setup
		auto instance = CreateInstanceAndSurface();
		auto physicalDevice = CreatePhysicalDevice( instance );
		FinalizeAndCreateDevice( physicalDevice );

		// Resources
		CreateSwapchain();
		CreateCommands();
		CreateSyncStructures();
		CreateDescriptors();
	}

	//
	// Create vulkan memory allocator instance
	//
	{
		CreateAllocator();
	}

	m_hasInitialized = true;
	return STATUS_OK;
}

RenderContextStatus VulkanRenderContext::Shutdown()
{
	ErrorIf( !m_hasInitialized, RenderContextStatus::NOT_INITIALIZED );

	m_hasInitialized = false;
	return STATUS_OK;
}

RenderContextStatus VulkanRenderContext::BeginRendering()
{
	ErrorIf( !m_hasInitialized, RenderContextStatus::NOT_INITIALIZED );
	ErrorIf( m_renderingActive, RenderContextStatus::BEGIN_END_MISMATCH );

	m_renderingActive = true;
	return STATUS_OK;
}

RenderContextStatus VulkanRenderContext::EndRendering()
{
	ErrorIf( !m_hasInitialized, RenderContextStatus::NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RenderContextStatus::BEGIN_END_MISMATCH );

	m_renderingActive = false;
	return STATUS_OK;
}