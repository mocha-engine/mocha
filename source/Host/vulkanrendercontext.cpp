#include "vulkanrendercontext.h"

#include <mesh.h>
#include <pipeline.h>
#include <shadercompiler.h>
#include <volk.h>

#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>

// ----------------------------------------------------------------------------------------------------------------------------

void VulkanSwapchain::CreateMainSwapchain( Size2D size )
{
	vkb::SwapchainBuilder swapchainBuilder( m_parent->m_chosenGPU, m_parent->m_device, m_parent->m_surface );

	vkb::Swapchain vkbSwapchain = swapchainBuilder.set_old_swapchain( m_swapchain )
	                                  .set_desired_format( { VK_FORMAT_R8G8B8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	                                  .set_desired_present_mode( VK_PRESENT_MODE_MAILBOX_KHR )
	                                  .set_desired_extent( size.x, size.y )
	                                  .build()
	                                  .value();

	m_swapchain = vkbSwapchain.swapchain;

	m_swapchainTextures = {};
	auto images = vkbSwapchain.get_images().value();
	auto imageViews = vkbSwapchain.get_image_views().value();
	auto imageFormat = vkbSwapchain.image_format;

	for ( uint32_t i = 0; i < vkbSwapchain.image_count; ++i )
	{
		VulkanRenderTexture renderTexture( m_parent );
		renderTexture.image = images[i];
		renderTexture.imageView = imageViews[i];
		renderTexture.format = imageFormat;

		m_swapchainTextures.push_back( renderTexture );
	}
}

void VulkanSwapchain::CreateDepthTexture( Size2D size )
{
	RenderTextureInfo_t renderTextureInfo;
	renderTextureInfo.width = size.x;
	renderTextureInfo.height = size.y;
	renderTextureInfo.type = RENDER_TEXTURE_DEPTH;

	m_depthTexture = VulkanRenderTexture( m_parent, renderTextureInfo );
}

VulkanSwapchain::VulkanSwapchain( VulkanRenderContext* parent, Size2D size )
    : m_depthTexture( parent )
{
	SetParent( parent );

	CreateMainSwapchain( size );
	CreateDepthTexture( size );
}

void VulkanSwapchain::Update( Size2D newSize )
{
	CreateMainSwapchain( newSize );
	CreateDepthTexture( newSize );
}

// ----------------------------------------------------------------------------------------------------------------------------

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
		return VK_FORMAT_D32_SFLOAT_S8_UINT;
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

VulkanRenderTexture::VulkanRenderTexture( VulkanRenderContext* parent, RenderTextureInfo_t textureInfo )
{
	SetParent( parent );

	VkExtent3D depthImageExtent = {
	    textureInfo.width,
	    textureInfo.height,
	    1,
	};

	format = GetFormat( textureInfo.type ); // Depth & stencil format

	VkImageCreateInfo imageInfo = VKInit::ImageCreateInfo( format, GetUsageFlagBits( textureInfo.type ), depthImageExtent, 1 );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	allocInfo.requiredFlags = VkMemoryPropertyFlags( VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT );

	vmaCreateImage( m_parent->m_allocator, &imageInfo, &allocInfo, &image, &allocation, nullptr );

	VkImageViewCreateInfo viewInfo = VKInit::ImageViewCreateInfo( format, image, GetAspectFlags( textureInfo.type ), 1 );
	VK_CHECK( vkCreateImageView( parent->m_device, &viewInfo, nullptr, &imageView ) );
}

// ----------------------------------------------------------------------------------------------------------------------------

VulkanImageTexture::VulkanImageTexture( VulkanRenderContext* parent, ImageTextureInfo_t textureInfo )
{
	SetParent( parent );
}

void VulkanImageTexture::SetData( TextureData_t textureData )
{
	VkFormat imageFormat = ( VkFormat )textureData.imageFormat;
	VkDeviceSize imageSize = 0;

	for ( int i = 0; i < textureData.mipCount; ++i )
	{
		imageSize += CalcMipSize( textureData.width, textureData.height, i, imageFormat );
	}

	BufferInfo_t bufferInfo = {};
	bufferInfo.size = imageSize;
	bufferInfo.type = BUFFER_TYPE_STAGING;
	bufferInfo.usage = BUFFER_USAGE_FLAG_TRANSFER_SRC;

	Handle bufferHandle;
	m_parent->CreateBuffer( bufferInfo, &bufferHandle );

	std::shared_ptr<VulkanBuffer> stagingBuffer = m_parent->m_buffers.Get( bufferHandle );

	void* mappedData;
	vmaMapMemory( m_parent->m_allocator, stagingBuffer->allocation, &mappedData );
	memcpy( mappedData, textureData.mipData.data, static_cast<size_t>( imageSize ) );
	vmaUnmapMemory( m_parent->m_allocator, stagingBuffer->allocation );

	VkExtent3D imageExtent;
	imageExtent.width = textureData.width;
	imageExtent.height = textureData.height;
	imageExtent.depth = 1;

	VkImageCreateInfo imageCreateInfo = VKInit::ImageCreateInfo(
	    imageFormat, VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT, imageExtent, textureData.mipCount );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_AUTO;

	vmaCreateImage( m_parent->m_allocator, &imageCreateInfo, &allocInfo, &image, &allocation, nullptr );

	m_parent->ImmediateSubmit( [&]( VkCommandBuffer cmd ) -> RenderStatus {
		VkImageSubresourceRange range;
		range.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		range.baseMipLevel = 0;
		range.levelCount = textureData.mipCount;
		range.baseArrayLayer = 0;
		range.layerCount = 1;

		VkImageMemoryBarrier imageBarrier_toTransfer = {};
		imageBarrier_toTransfer.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;

		imageBarrier_toTransfer.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		imageBarrier_toTransfer.newLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toTransfer.image = image;
		imageBarrier_toTransfer.subresourceRange = range;

		imageBarrier_toTransfer.srcAccessMask = 0;
		imageBarrier_toTransfer.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;

		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0, nullptr,
		    1, &imageBarrier_toTransfer );

		std::vector<VkBufferImageCopy> mipRegions = {};

		for ( size_t mip = 0; mip < textureData.mipCount; mip++ )
		{
			//
			// Calculate the number of bytes that have passed until this mip
			// This is done by taking all past mip widths * heights
			//
			VkDeviceSize bufferOffset = 0;

			for ( size_t i = 0; i < mip; ++i )
			{
				// Calculate the width & height of this mip
				uint32_t mipWidth, mipHeight;
				GetMipDimensions( textureData.width, textureData.height, i, &mipWidth, &mipHeight );
				bufferOffset += CalcMipSize( textureData.width, textureData.height, i, imageFormat );
			}

			spdlog::trace(
			    "Offset for mip {} on texture size {}x{} is {}", mip, textureData.width, textureData.height, bufferOffset );

			VkExtent3D mipExtent;
			GetMipDimensions( textureData.width, textureData.height, mip, &mipExtent.width, &mipExtent.height );
			mipExtent.depth = 1;

			VkBufferImageCopy copyRegion = {};
			copyRegion.bufferOffset = bufferOffset;
			copyRegion.bufferRowLength = 0;
			copyRegion.bufferImageHeight = 0;

			copyRegion.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
			copyRegion.imageSubresource.mipLevel = mip;
			copyRegion.imageSubresource.baseArrayLayer = 0;
			copyRegion.imageSubresource.layerCount = 1;
			copyRegion.imageExtent = mipExtent;

			mipRegions.push_back( copyRegion );
		}

		vkCmdCopyBufferToImage(
		    cmd, stagingBuffer->buffer, image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, mipRegions.size(), mipRegions.data() );

		VkImageMemoryBarrier imageBarrier_toReadable = imageBarrier_toTransfer;

		imageBarrier_toReadable.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toReadable.newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;

		imageBarrier_toReadable.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		imageBarrier_toReadable.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;

		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
		    nullptr, 1, &imageBarrier_toReadable );

		return RENDER_STATUS_OK;
	} );

	VkImageViewCreateInfo imageViewInfo =
	    VKInit::ImageViewCreateInfo( ( VkFormat )imageFormat, image, VK_IMAGE_ASPECT_COLOR_BIT, textureData.mipCount );
	vkCreateImageView( m_parent->m_device, &imageViewInfo, nullptr, &imageView );

	spdlog::info( "Created texture with size {}x{}", textureData.width, textureData.height );
}
void VulkanImageTexture::Copy( TextureCopyData_t copyData ) {}

// ----------------------------------------------------------------------------------------------------------------------------

VulkanCommandContext::VulkanCommandContext( VulkanRenderContext* parent )
{
	SetParent( parent );

	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.pNext = nullptr;

	poolInfo.queueFamilyIndex = parent->m_graphicsQueueFamily;
	poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

	VK_CHECK( vkCreateCommandPool( parent->m_device, &poolInfo, nullptr, &commandPool ) );

	VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.pNext = nullptr;

	allocInfo.commandPool = commandPool;
	allocInfo.commandBufferCount = 1;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

	VK_CHECK( vkAllocateCommandBuffers( parent->m_device, &allocInfo, &commandBuffer ) );

	VkFenceCreateInfo fenceInfo = {};
	fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceInfo.pNext = nullptr;
	fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;

	VK_CHECK( vkCreateFence( parent->m_device, &fenceInfo, nullptr, &fence ) );
}

// ----------------------------------------------------------------------------------------------------------------------------

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

VulkanSampler::VulkanSampler( VulkanRenderContext* parent, SamplerType samplerType )
{
	SetParent( parent );

	VkSamplerCreateInfo samplerInfo = GetCreateInfo( samplerType );
	VK_CHECK( vkCreateSampler( parent->m_device, &samplerInfo, nullptr, &sampler ) );
}

// ----------------------------------------------------------------------------------------------------------------------------

VulkanBuffer::VulkanBuffer( VulkanRenderContext* parent, BufferInfo_t bufferInfo, VmaMemoryUsage memoryUsage )
{
	SetParent( parent );

	VkBufferCreateInfo bufferCreateInfo = {};
	bufferCreateInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferCreateInfo.pNext = nullptr;

	bufferCreateInfo.size = bufferInfo.size;
	bufferCreateInfo.usage = GetBufferUsageFlags( bufferInfo );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = memoryUsage;
	allocInfo.flags = VMA_ALLOCATION_CREATE_HOST_ACCESS_SEQUENTIAL_WRITE_BIT;

	VK_CHECK( vmaCreateBuffer( m_parent->m_allocator, &bufferCreateInfo, &allocInfo, &buffer, &allocation, nullptr ) );
}

VkBufferUsageFlags VulkanBuffer::GetBufferUsageFlags( BufferInfo_t bufferInfo )
{
	VkBufferUsageFlags outFlags = 0;

	if ( ( bufferInfo.usage & BUFFER_USAGE_FLAG_VERTEX_BUFFER ) != 0 )
		outFlags |= VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;

	if ( ( bufferInfo.usage & BUFFER_USAGE_FLAG_INDEX_BUFFER ) != 0 )
		outFlags |= VK_BUFFER_USAGE_INDEX_BUFFER_BIT;

	if ( ( bufferInfo.usage & BUFFER_USAGE_FLAG_UNIFORM_BUFFER ) != 0 )
		outFlags |= VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;

	if ( ( bufferInfo.usage & BUFFER_USAGE_FLAG_TRANSFER_SRC ) != 0 )
		outFlags |= VK_BUFFER_USAGE_TRANSFER_SRC_BIT;

	if ( ( bufferInfo.usage & BUFFER_USAGE_FLAG_TRANSFER_DST ) != 0 )
		outFlags |= VK_BUFFER_USAGE_TRANSFER_DST_BIT;

	if ( bufferInfo.type == BUFFER_TYPE_VERTEX_INDEX_DATA )
		assert( ( outFlags & VK_BUFFER_USAGE_INDEX_BUFFER_BIT ) != 0 || ( outFlags & VK_BUFFER_USAGE_VERTEX_BUFFER_BIT ) != 0 );

	assert( outFlags != 0 && "Flags cannot be 0" );

	return outFlags;
}

void VulkanBuffer::SetData( BufferUploadInfo_t uploadInfo )
{
	struct AllocatedBuffer
	{
		VkBuffer buffer;
		VmaAllocation allocation;
	};

	VkBufferCreateInfo stagingBufferInfo = {};
	stagingBufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	stagingBufferInfo.pNext = nullptr;

	stagingBufferInfo.size = uploadInfo.data.size;
	stagingBufferInfo.usage = VK_BUFFER_USAGE_TRANSFER_SRC_BIT;

	VmaAllocationCreateInfo vmaallocInfo = {};
	AllocatedBuffer stagingBuffer = {};

	vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_ONLY;

	VK_CHECK( vmaCreateBuffer(
	    m_parent->m_allocator, &stagingBufferInfo, &vmaallocInfo, &stagingBuffer.buffer, &stagingBuffer.allocation, nullptr ) );

	void* data;
	vmaMapMemory( m_parent->m_allocator, stagingBuffer.allocation, &data );
	memcpy( data, uploadInfo.data.data, uploadInfo.data.size );
	vmaUnmapMemory( m_parent->m_allocator, stagingBuffer.allocation );

	m_parent->ImmediateSubmit( [=]( VkCommandBuffer cmd ) -> RenderStatus {
		VkBufferCopy copy = {};
		copy.dstOffset = 0;
		copy.srcOffset = 0;
		copy.size = uploadInfo.data.size;

		vkCmdCopyBuffer( cmd, stagingBuffer.buffer, buffer, 1, &copy );

		return RENDER_STATUS_OK;
	} );
}

// ----------------------------------------------------------------------------------------------------------------------------

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

	m_window = std::make_unique<Window>( 1280, 720 );
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
		ErrorMessage( errorStr );

		// Exit
		exit( error.type.value() );
	}

	return physicalDeviceReturn.value();
}

void VulkanRenderContext::CreateSwapchain()
{
	Size2D size = m_window->GetWindowSize();

	m_swapchain = VulkanSwapchain( this, size );

	m_window->m_onWindowResized = [&]( Size2D newSize ) { m_swapchain.Update( newSize ); };
}

void VulkanRenderContext::CreateCommands()
{
	m_mainContext = VulkanCommandContext( this );

	m_uploadContext = VulkanCommandContext( this );
	vkResetFences( m_device, 1, &m_uploadContext.fence );
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
	m_pointSampler = VulkanSampler( this, SAMPLER_TYPE_POINT );
	m_anisoSampler = VulkanSampler( this, SAMPLER_TYPE_ANISOTROPIC );
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

RenderStatus VulkanRenderContext::Startup()
{
	ErrorIf( m_hasInitialized, RENDER_STATUS_ALREADY_INITIALIZED );

	volkInitialize();

	//
	// Create vulkan objects
	//
	{
		// Initial setup
		auto instance = CreateInstanceAndSurface();
		auto physicalDevice = CreatePhysicalDevice( instance );
		FinalizeAndCreateDevice( physicalDevice );
		CreateAllocator();

		// Resources
		CreateSamplers();
		CreateSwapchain();
		CreateCommands();
		CreateSyncStructures();
		CreateDescriptors();
	}

	m_hasInitialized = true;
	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::Shutdown()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	m_hasInitialized = false;
	return RENDER_STATUS_OK;
}

inline bool VulkanRenderContext::CanRender()
{
	// Get window size ( we use this in a load of places )
	Size2D renderSize = m_window->GetWindowSize();

	if ( renderSize.x < 1 || renderSize.y < 1 )
	{
		// Do not render if we can't render to anything..
		return false;
	}

	return true;
}

RenderStatus VulkanRenderContext::BeginRendering()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	// Get window size ( we use this in a load of places )
	Size2D renderSize = m_window->GetWindowSize();

	if ( !CanRender() )
	{
		m_renderingActive = true;
		return RENDER_STATUS_OK; // We handled this internally, so don't return an error.
	}

	// Wait until we can render ( 1 second timeout )
	VK_CHECK( vkWaitForFences( m_device, 1, &m_mainContext.fence, true, 1000000000 ) );
	VK_CHECK( vkResetFences( m_device, 1, &m_mainContext.fence ) );

	// Acquire swapchain image ( 1 second timeout )
	m_swapchainImageIndex = m_swapchain.AcquireSwapchainImageIndex( m_device, m_presentSemaphore, m_mainContext );

	m_swapchainTarget = m_swapchain.m_swapchainTextures[m_swapchainImageIndex];
	m_depthTarget = m_swapchain.m_depthTexture;

	// Begin command buffer
	VkCommandBuffer cmd = m_mainContext.commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo( VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT );
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	//
	// Set viewport & scissor
	//
	VkViewport viewport = {};
	viewport.minDepth = 0.0;
	viewport.maxDepth = 1.0;
	viewport.width = renderSize.x;
	viewport.height = renderSize.y;

	VkRect2D scissor = { { 0, 0 }, { renderSize.x, renderSize.y } };
	vkCmdSetScissor( cmd, 0, 1, &scissor );
	vkCmdSetViewport( cmd, 0, 1, &viewport );

	//
	// We want to draw the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL before presenting
	//
	VkImageMemoryBarrier startRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier(
	    VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, m_swapchainTarget.image );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, 0, 0, nullptr,
	    0, nullptr, 1, &startRenderImageMemoryBarrier );

	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkClearValue depthClear = {};
	depthClear.depthStencil.depth = 1.0f;

	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_swapchainTarget.imageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	colorAttachmentInfo.clearValue = colorClear;

	VkRenderingAttachmentInfo depthAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_depthTarget.imageView, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL );
	depthAttachmentInfo.clearValue = depthClear;

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( &colorAttachmentInfo, &depthAttachmentInfo, renderSize );
	vkCmdBeginRendering( cmd, &renderInfo );

	m_renderingActive = true;
	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::EndRendering()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	VkCommandBuffer cmd = m_mainContext.commandBuffer;
	vkCmdEndRendering( cmd );

	//
	// We want to present the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_PRESENT_SRC_KHR before presenting
	//
	VkImageMemoryBarrier endRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier(
	    VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_PRESENT_SRC_KHR, m_swapchainTarget.image );

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

	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, m_mainContext.fence ) );

	// Present
	VkPresentInfoKHR presentInfo = VKInit::PresentInfo( &m_swapchain.m_swapchain, &m_renderSemaphore, &m_swapchainImageIndex );

	if ( !CanRender() )
	{
		m_renderingActive = false;
		return RENDER_STATUS_OK; // We handled this internally, so don't return an error.
	}

	VK_CHECK( vkQueuePresentKHR( m_graphicsQueue, &presentInfo ) );

	m_renderingActive = false;
	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindPipeline( Pipeline p )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	m_pipeline = m_pipelines.Get( p.m_handle );

	VulkanPipeline pipeline = *m_pipeline.get();

	vkCmdBindPipeline( m_mainContext.commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, pipeline.pipeline );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindDescriptor( Descriptor d )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	VulkanDescriptor descriptor = *m_descriptors.Get( d.m_handle ).get();

	vkCmdBindDescriptorSets( m_mainContext.commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipeline->layout, 0, 1,
	    &descriptor.descriptorSet, 0, nullptr );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::UpdateDescriptor( Descriptor d, DescriptorUpdateInfo_t updateInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	BindDescriptor( d );

	VulkanDescriptor descriptor = *m_descriptors.Get( d.m_handle ).get();
	VulkanImageTexture texture = *m_imageTextures.Get( updateInfo.src->m_handle ).get();

	VkDescriptorImageInfo imageInfo = {};
	imageInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
	imageInfo.imageView = texture.imageView;
	imageInfo.sampler = SAMPLER_TYPE_ANISOTROPIC ? m_anisoSampler.sampler : m_pointSampler.sampler; // TODO

	auto descriptorWrite = VKInit::WriteDescriptorImage(
	    VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, descriptor.descriptorSet, &imageInfo, updateInfo.binding );

	vkUpdateDescriptorSets( m_device, 1, &descriptorWrite, 0, nullptr );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindVertexBuffer( VertexBuffer vb )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	VulkanBuffer vertexBuffer = *m_buffers.Get( vb.m_handle ).get();

	VkDeviceSize offset = 0;
	vkCmdBindVertexBuffers( m_mainContext.commandBuffer, 0, 1, &vertexBuffer.buffer, &offset );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindIndexBuffer( IndexBuffer ib )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	VulkanBuffer indexBuffer = *m_buffers.Get( ib.m_handle ).get();

	VkDeviceSize offset = 0;
	vkCmdBindIndexBuffer( m_mainContext.commandBuffer, indexBuffer.buffer, offset, VK_INDEX_TYPE_UINT32 );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindConstants( RenderPushConstants p )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	vkCmdPushConstants( m_mainContext.commandBuffer, m_pipeline->layout,
	    VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT, 0, sizeof( RenderPushConstants ), &p );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	vkCmdDrawIndexed( m_mainContext.commandBuffer, indexCount, instanceCount, 0, 0, 0 );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindRenderTarget( RenderTexture rt )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	assert( "TODO" );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::GetRenderSize( Size2D* outSize )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	*outSize = m_window->GetWindowSize();

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::UpdateWindow()
{
	m_window->Update();

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::RenderMesh( RenderPushConstants constants, Mesh* mesh )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	// JIT pipeline creation
	if ( !mesh->material.m_pipeline.IsValid() )
	{
		spdlog::trace( "VulkanRenderContext::RenderMesh - Handle wasn't valid, creating JIT render pipeline..." );

		mesh->material.CreateResources();
	}

	BindPipeline( mesh->material.m_pipeline );
	BindDescriptor( mesh->material.m_descriptor );

	for ( int i = 0; i < mesh->material.m_textures.size(); ++i )
	{
		DescriptorUpdateInfo_t updateInfo = {};
		updateInfo.binding = i;
		updateInfo.samplerType = SAMPLER_TYPE_POINT;
		updateInfo.src = &mesh->material.m_textures[i].m_image;

		UpdateDescriptor( mesh->material.m_descriptor, updateInfo );
	}

	BindConstants( constants );
	BindVertexBuffer( mesh->vertexBuffer );
	BindIndexBuffer( mesh->indexBuffer );

	Draw( mesh->vertices.count, mesh->indices.count, 1 );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateImageTexture( ImageTextureInfo_t textureInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanImageTexture imageTexture( this, textureInfo );

	*outHandle = m_imageTextures.Add( imageTexture );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateRenderTexture( RenderTextureInfo_t textureInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanRenderTexture renderTexture( this, textureInfo );

	*outHandle = m_renderTextures.Add( renderTexture );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::SetImageTextureData( Handle handle, TextureData_t pipelineInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanImageTexture* imageTexture = m_imageTextures.Get( handle ).get();

	if ( imageTexture == nullptr )
		return RENDER_STATUS_INVALID_HANDLE;

	imageTexture->SetData( pipelineInfo );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanImageTexture* imageTexture = m_imageTextures.Get( handle ).get();

	if ( imageTexture == nullptr )
		return RENDER_STATUS_INVALID_HANDLE;

	imageTexture->Copy( pipelineInfo );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateBuffer( BufferInfo_t bufferInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	VulkanBuffer buffer( this, bufferInfo, VMA_MEMORY_USAGE_AUTO );

	*outHandle = m_buffers.Add( buffer );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateVertexBuffer( BufferInfo_t bufferInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	VulkanBuffer buffer( this, bufferInfo, VMA_MEMORY_USAGE_AUTO );

	*outHandle = m_buffers.Add( buffer );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateIndexBuffer( BufferInfo_t bufferInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	VulkanBuffer buffer( this, bufferInfo, VMA_MEMORY_USAGE_AUTO );

	*outHandle = m_buffers.Add( buffer );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::UploadBuffer( Handle handle, BufferUploadInfo_t pipelineInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanBuffer* buffer = m_buffers.Get( handle ).get();

	if ( buffer == nullptr )
		return RENDER_STATUS_INVALID_HANDLE;

	buffer->SetData( pipelineInfo );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreatePipeline( PipelineInfo_t pipelineInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanPipeline pipeline( this, pipelineInfo );

	*outHandle = m_pipelines.Add( pipeline );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateDescriptor( DescriptorInfo_t pipelineInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanDescriptor descriptor( this, pipelineInfo );

	*outHandle = m_descriptors.Add( descriptor );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CreateShader( ShaderInfo_t pipelineInfo, Handle* outHandle )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	VulkanShader shader( this, pipelineInfo );

	*outHandle = m_shaders.Add( shader );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::ImmediateSubmit( std::function<RenderStatus( VkCommandBuffer commandBuffer )> func )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	RenderStatus status;

	VkCommandBuffer cmd = m_uploadContext.commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo( VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT );
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	status = func( cmd );

	VK_CHECK( vkEndCommandBuffer( cmd ) );

	VkSubmitInfo submit = VKInit::SubmitInfo( &cmd );
	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, m_uploadContext.fence ) );

	vkWaitForFences( m_device, 1, &m_uploadContext.fence, true, 9999999999 );
	vkResetFences( m_device, 1, &m_uploadContext.fence );

	vkResetCommandPool( m_device, m_uploadContext.commandPool, 0 );

	return status;
}

// ----------------------------------------------------------------------------------------------------------------------------

void VulkanShader::LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule )
{
	VkDevice device = m_parent->m_device;

	std::string line, text;
	std::ifstream in( filePath );

	while ( std::getline( in, line ) )
	{
		text += line + "\n";
	}

	const char* buffer = text.c_str();

	std::vector<unsigned int> shaderBits;
	if ( !ShaderCompiler::Instance().Compile( shaderStage, buffer, shaderBits ) )
	{
		std::string error = std::string( filePath ) + " failed to compile.\nCheck the console for more details.";
		ErrorMessage( error );
		exit( 1 );
	}

	//
	//
	//

	VkShaderModuleCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.pNext = nullptr;

	createInfo.codeSize = shaderBits.size() * sizeof( uint32_t );
	createInfo.pCode = shaderBits.data();

	VkShaderModule shaderModule;

	if ( vkCreateShaderModule( device, &createInfo, nullptr, &shaderModule ) != VK_SUCCESS )
	{
		spdlog::error( "Could not compile shader {}", filePath );
		return;
	}

	*outShaderModule = shaderModule;
}

VulkanShader::VulkanShader( VulkanRenderContext* parent, ShaderInfo_t shaderInfo )
{
	SetParent( parent );

	LoadShaderModule( shaderInfo.shaderPath.c_str(), VK_SHADER_STAGE_FRAGMENT_BIT, &fragmentShader );
	spdlog::info( "VulkanShader::VulkanShader: Fragment shader compiled successfully" );

	LoadShaderModule( shaderInfo.shaderPath.c_str(), VK_SHADER_STAGE_VERTEX_BIT, &vertexShader );
	spdlog::info( "VulkanShader::VulkanShader: Vertex shader compiled successfully" );
}

// ----------------------------------------------------------------------------------------------------------------------------

VulkanDescriptor::VulkanDescriptor( VulkanRenderContext* parent, DescriptorInfo_t descriptorInfo )
{
	SetParent( parent );

	std::vector<VkDescriptorSetLayoutBinding> bindings = {};

	for ( int i = 0; i < descriptorInfo.bindings.size(); ++i )
	{
		VkDescriptorSetLayoutBinding binding = {};
		binding.binding = i;
		binding.descriptorType = GetDescriptorType( descriptorInfo.bindings[i].type );
		binding.descriptorCount = 1;
		binding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT | VK_SHADER_STAGE_VERTEX_BIT;
		binding.pImmutableSamplers = nullptr;

		bindings.push_back( binding );
	}

	VkDescriptorSetLayoutCreateInfo layoutInfo = VKInit::DescriptorSetLayoutCreateInfo( bindings.data(), bindings.size() );
	VK_CHECK( vkCreateDescriptorSetLayout( m_parent->m_device, &layoutInfo, nullptr, &descriptorSetLayout ) );

	VkDescriptorSetAllocateInfo allocInfo =
	    VKInit::DescriptorSetAllocateInfo( m_parent->m_descriptorPool, &descriptorSetLayout, 1 );

	VK_CHECK( vkAllocateDescriptorSets( m_parent->m_device, &allocInfo, &descriptorSet ) );
}

VkDescriptorType VulkanDescriptor::GetDescriptorType( DescriptorBindingType type )
{
	switch ( type )
	{
	case DESCRIPTOR_BINDING_TYPE_IMAGE:
		return VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
	}

	assert( false && "Invalid descriptor binding type" );
}

// ----------------------------------------------------------------------------------------------------------------------------

VkFormat VulkanPipeline::GetVulkanFormat( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VERTEX_ATTRIBUTE_FORMAT_INT:
		return VK_FORMAT_R32_SINT;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT:
		return VK_FORMAT_R32_SFLOAT;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT2:
		return VK_FORMAT_R32G32_SFLOAT;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT3:
		return VK_FORMAT_R32G32B32_SFLOAT;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT4:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
		break;
	}

	return VK_FORMAT_UNDEFINED;
}

uint32_t VulkanPipeline::GetSizeOf( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VERTEX_ATTRIBUTE_FORMAT_INT:
		return sizeof( int );
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT:
		return sizeof( float );
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT2:
		return sizeof( float ) * 2;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT3:
		return sizeof( float ) * 3;
		break;
	case VERTEX_ATTRIBUTE_FORMAT_FLOAT4:
		return sizeof( float ) * 4;
		break;
	}

	return 0;
}

VulkanPipeline::VulkanPipeline( VulkanRenderContext* parent, PipelineInfo_t pipelineInfo )
{
	SetParent( parent );

	PipelineBuilder builder;

	VkPipelineLayoutCreateInfo pipeline_layout_info = VKInit::PipelineLayoutCreateInfo();
	VkPushConstantRange push_constant = {};

	push_constant.offset = 0;
	push_constant.size = sizeof( RenderPushConstants );
	push_constant.stageFlags = VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT;

	pipeline_layout_info.pPushConstantRanges = &push_constant;
	pipeline_layout_info.pushConstantRangeCount = 1;

	std::vector<VkDescriptorSetLayout> setLayouts;

	for ( auto& descriptor : pipelineInfo.descriptors )
	{
		VulkanDescriptor vkDescriptor = *m_parent->m_descriptors.Get( descriptor->m_handle ).get();

		setLayouts.push_back( vkDescriptor.descriptorSetLayout );
	}

	pipeline_layout_info.pSetLayouts = setLayouts.data();
	pipeline_layout_info.setLayoutCount = setLayouts.size();

	VK_CHECK( vkCreatePipelineLayout( m_parent->m_device, &pipeline_layout_info, nullptr, &builder.m_pipelineLayout ) );

	layout = builder.m_pipelineLayout;

	builder.m_rasterizer = VKInit::PipelineRasterizationStateCreateInfo( VK_POLYGON_MODE_FILL );
	builder.m_multisampling = VKInit::PipelineMultisampleStateCreateInfo();
	builder.m_colorBlendAttachment = VKInit::PipelineColorBlendAttachmentState();

	VulkanShader shader( m_parent, pipelineInfo.shaderInfo );

	std::vector<VkPipelineShaderStageCreateInfo> shaderStages;

	shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_FRAGMENT_BIT, shader.fragmentShader ) );
	shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_VERTEX_BIT, shader.vertexShader ) );

	builder.m_shaderStages = shaderStages;

	// Calculate stride size
	size_t stride = 0;
	for ( int i = 0; i < pipelineInfo.vertexAttributes.size(); ++i )
	{
		stride += GetSizeOf( ( VertexAttributeFormat )pipelineInfo.vertexAttributes[i].format );
	}

	VulkanVertexInputDescription description = {};

	VkVertexInputBindingDescription mainBinding = {};
	mainBinding.binding = 0;
	mainBinding.stride = stride;
	mainBinding.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

	description.bindings.push_back( mainBinding );

	size_t offset = 0;

	for ( int i = 0; i < pipelineInfo.vertexAttributes.size(); ++i )
	{
		auto attribute = pipelineInfo.vertexAttributes[i];

		VkVertexInputAttributeDescription positionAttribute = {};
		positionAttribute.binding = 0;
		positionAttribute.location = i;
		positionAttribute.format = GetVulkanFormat( ( VertexAttributeFormat )attribute.format );
		positionAttribute.offset = offset;
		description.attributes.push_back( positionAttribute );

		offset += GetSizeOf( ( VertexAttributeFormat )attribute.format );
	}

	builder.m_vertexInputInfo = VKInit::PipelineVertexInputStateCreateInfo();
	builder.m_vertexInputInfo.pVertexAttributeDescriptions = description.attributes.data();
	builder.m_vertexInputInfo.vertexAttributeDescriptionCount = static_cast<uint32_t>( description.attributes.size() );

	builder.m_vertexInputInfo.pVertexBindingDescriptions = description.bindings.data();
	builder.m_vertexInputInfo.vertexBindingDescriptionCount = static_cast<uint32_t>( description.bindings.size() );

	builder.m_inputAssembly = VKInit::PipelineInputAssemblyStateCreateInfo( VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST );
	builder.m_depthStencil =
	    VKInit::DepthStencilCreateInfo( !pipelineInfo.ignoreDepth, !pipelineInfo.ignoreDepth, VK_COMPARE_OP_LESS_OR_EQUAL );

	pipeline = builder.Build(
	    m_parent->m_device, m_parent->m_swapchain.m_swapchainTextures[0].format, m_parent->m_swapchain.m_depthTexture.format );
}
