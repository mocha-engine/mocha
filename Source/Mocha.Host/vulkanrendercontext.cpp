#include "vulkanrendercontext.h"

#include <hostmanager.h>
#include <mesh.h>
#include <pipeline.h>
#include <projectmanager.h>
#include <root.h>
#include <shadercompiler.h>
#include <volk.h>

#define VMA_DEBUG_LOG( format, ... )                     \
	{                                                    \
		/* Use snprintf->spdlog::trace */                \
		char buffer[1024];                               \
		snprintf( buffer, 1024, format, ##__VA_ARGS__ ); \
		spdlog::trace( buffer );                         \
	}

#define VMA_IMPLEMENTATION
#include <vk_mem_alloc.h>

#ifdef _IMGUI
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <fontawesome.h>
#include <imgui.h>
#include <implot.h>
#endif

// ----------------------------------------------------------------------------------------------------------------------------

void VulkanObject::SetDebugName( const char* name, VkObjectType objectType, uint64_t handle )
{
	m_parent->SetDebugName( name, objectType, handle );
}

// ----------------------------------------------------------------------------------------------------------------------------

void VulkanSwapchain::CreateMainSwapchain( Size2D size )
{
	vkb::SwapchainBuilder swapchainBuilder( m_parent->m_chosenGPU, m_parent->m_device, m_parent->m_surface );

	vkb::Swapchain vkbSwapchain = swapchainBuilder.set_old_swapchain( m_swapchain )
	                                  .set_desired_format( { VK_FORMAT_B8G8R8A8_UNORM, VK_COLOR_SPACE_SRGB_NONLINEAR_KHR } )
	                                  .set_desired_present_mode( VK_PRESENT_MODE_MAILBOX_KHR )
	                                  .set_desired_extent( size.x, size.y )
	                                  .build()
	                                  .value();

	m_swapchain = vkbSwapchain.swapchain;
	SetDebugName( "Main Swapchain", VK_OBJECT_TYPE_SWAPCHAIN_KHR, ( uint64_t )m_swapchain );

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

		SetDebugName( "Swapchain Texture", VK_OBJECT_TYPE_IMAGE, ( uint64_t )renderTexture.image );
		SetDebugName( "Swapchain Texture View", VK_OBJECT_TYPE_IMAGE_VIEW, ( uint64_t )renderTexture.imageView );

		m_swapchainTextures.emplace_back( renderTexture );
	}
}

VulkanSwapchain::VulkanSwapchain( VulkanRenderContext* parent, Size2D size )
{
	SetParent( parent );

	CreateMainSwapchain( size );
}

void VulkanSwapchain::Update( Size2D newSize )
{
	CreateMainSwapchain( newSize );
}

void VulkanSwapchain::Delete() const
{
	vkDestroySwapchainKHR( m_parent->m_device, m_swapchain, nullptr );
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

	__debugbreak(); // Invalid / unsupported Invalid render texture type
}

VkFormat VulkanRenderTexture::GetFormat( RenderTextureType type )
{
	switch ( type )
	{
	case RENDER_TEXTURE_COLOR:
	case RENDER_TEXTURE_COLOR_OPAQUE:
		// Some cards do not support alpha-less formats, so we use this for compatibility
		return VK_FORMAT_B8G8R8A8_UNORM;
	case RENDER_TEXTURE_DEPTH:
		return VK_FORMAT_D32_SFLOAT_S8_UINT;
	}

	__debugbreak(); // Invalid / unsupported render texture type
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

	__debugbreak(); // Invalid / unsupported render texture type
}

VulkanRenderTexture::VulkanRenderTexture( VulkanRenderContext* parent, RenderTextureInfo_t textureInfo )
{
	SetParent( parent );

	size = { textureInfo.width, textureInfo.height };

	VkExtent3D depthImageExtent = {
	    textureInfo.width,
	    textureInfo.height,
	    1,
	};

	format = GetFormat( textureInfo.type ); // Depth & stencil format

	VkImageCreateInfo imageInfo = VKInit::ImageCreateInfo(
	    format, GetUsageFlagBits( textureInfo.type ) | VK_IMAGE_USAGE_SAMPLED_BIT, depthImageExtent, 1 );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
	allocInfo.requiredFlags = VkMemoryPropertyFlags( VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT );

	vmaCreateImage( m_parent->m_allocator, &imageInfo, &allocInfo, &image, &allocation, nullptr );

	VkImageViewCreateInfo viewInfo = VKInit::ImageViewCreateInfo( format, image, GetAspectFlags( textureInfo.type ), 1 );
	VK_CHECK( vkCreateImageView( parent->m_device, &viewInfo, nullptr, &imageView ) );

	SetDebugName( "RenderTexture Image", VK_OBJECT_TYPE_IMAGE, ( uint64_t )image );
	SetDebugName( "RenderTexture Image View", VK_OBJECT_TYPE_IMAGE_VIEW, ( uint64_t )imageView );
}

void VulkanRenderTexture::Delete() const
{
	vkDestroyImageView( m_parent->m_device, imageView, nullptr );
	vmaDestroyImage( m_parent->m_allocator, image, allocation );
}
#pragma endregion
// ----------------------------------------------------------------------------------------------------------------------------

VulkanImageTexture::VulkanImageTexture( VulkanRenderContext* parent, ImageTextureInfo_t _textureInfo )
{
	SetParent( parent );

	textureInfo = _textureInfo;
}

void VulkanImageTexture::SetData( TextureData_t textureData )
{
	// Destroy old image
	Delete();

	VkFormat imageFormat = ( VkFormat )textureData.imageFormat;
	uint32_t imageSize = 0;

	for ( uint32_t i = 0; i < textureData.mipCount; ++i )
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
	memcpy( mappedData, textureData.mipData.data, static_cast<size_t>( textureData.mipData.size ) );
	vmaUnmapMemory( m_parent->m_allocator, stagingBuffer->allocation );

	VkExtent3D imageExtent;
	imageExtent.width = textureData.width;
	imageExtent.height = textureData.height;
	imageExtent.depth = 1;

	VkImageUsageFlags usageFlags = VK_IMAGE_USAGE_SAMPLED_BIT |
	                               /* We want to be able to copy to/from this image */
	                               VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;

	VkImageCreateInfo imageCreateInfo = VKInit::ImageCreateInfo( imageFormat, usageFlags, imageExtent, textureData.mipCount );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_AUTO;

	vmaCreateImage( m_parent->m_allocator, &imageCreateInfo, &allocInfo, &image, &allocation, nullptr );
	vmaSetAllocationName( m_parent->m_allocator, allocation, textureInfo.name.c_str() );

	m_parent->ImmediateSubmit( [&]( VkCommandBuffer cmd ) -> RenderStatus {
		{
			VkImageMemoryBarrier transitionBarrier =
			    VKInit::ImageMemoryBarrier( 0, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

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

		{
			VkImageMemoryBarrier transitionBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_TRANSFER_WRITE_BIT,
			    VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

		return RENDER_STATUS_OK;
	} );

	VkImageViewCreateInfo imageViewInfo =
	    VKInit::ImageViewCreateInfo( ( VkFormat )imageFormat, image, VK_IMAGE_ASPECT_COLOR_BIT, textureData.mipCount );
	vkCreateImageView( m_parent->m_device, &imageViewInfo, nullptr, &imageView );

	SetDebugName( textureInfo.name.c_str(), VK_OBJECT_TYPE_IMAGE, ( uint64_t )image );

	std::string imageViewName = textureInfo.name + " View";
	SetDebugName( imageViewName.c_str(), VK_OBJECT_TYPE_IMAGE_VIEW, ( uint64_t )imageView );
}

inline void VulkanImageTexture::TransitionLayout(
    VkCommandBuffer& cmd, VkImageLayout newLayout, VkAccessFlags newAccessMask, VkPipelineStageFlags stageMask )
{
	VkImageSubresourceRange range;
	range.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	range.baseMipLevel = 0;
	range.levelCount = VK_REMAINING_MIP_LEVELS;
	range.baseArrayLayer = 0;
	range.layerCount = 1;

	VkImageMemoryBarrier imageBarrier = {};
	imageBarrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;

	imageBarrier.oldLayout = currentLayout;
	imageBarrier.newLayout = newLayout;
	imageBarrier.image = image;
	imageBarrier.subresourceRange = range;

	imageBarrier.srcAccessMask = currentAccessMask;
	imageBarrier.dstAccessMask = newAccessMask;

	vkCmdPipelineBarrier( cmd, currentStageMask, stageMask, 0, 0, nullptr, 0, nullptr, 1, &imageBarrier );

	currentStageMask = stageMask;
	currentAccessMask = newAccessMask;
	currentLayout = newLayout;
}

void VulkanImageTexture::Copy( TextureCopyData_t copyData )
{
	m_parent->ImmediateSubmit( [&]( VkCommandBuffer cmd ) -> RenderStatus {
		auto src = m_parent->m_imageTextures.Get( copyData.src->m_handle );

		//
		// Transition source image to VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL
		//
		{
			VkImageMemoryBarrier transitionBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_SHADER_READ_BIT,
			    VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, src->image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

		//
		// Transition destination image to VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL
		//
		{
			VkImageMemoryBarrier transitionBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_SHADER_READ_BIT,
			    VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

		//
		// Copy image
		//
		VkImageSubresourceLayers srcSubresource = {};
		srcSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		srcSubresource.mipLevel = 0;
		srcSubresource.baseArrayLayer = 0;
		srcSubresource.layerCount = 1;

		VkImageSubresourceLayers dstSubresource = srcSubresource;

		VkOffset3D srcOffset = { ( int32_t )copyData.srcX, ( int32_t )copyData.srcY, 0 };
		VkOffset3D dstOffset = { ( int32_t )copyData.dstX, ( int32_t )copyData.dstY, 0 };

		VkExtent3D extent = { copyData.width, copyData.height, 1 };

		VkImageBlit region = {};
		region.srcSubresource = srcSubresource;
		region.srcOffsets[0] = srcOffset;
		region.srcOffsets[1] = { srcOffset.x + ( int32_t )copyData.width, srcOffset.y + ( int32_t )copyData.height, 1 };
		region.dstSubresource = dstSubresource;
		region.dstOffsets[0] = dstOffset;
		region.dstOffsets[1] = { dstOffset.x + ( int32_t )copyData.width, dstOffset.y + ( int32_t )copyData.height, 1 };

		vkCmdBlitImage( cmd, src->image, VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1,
		    &region, VK_FILTER_NEAREST );

		//
		// Return images to initial layouts
		//
		{
			VkImageMemoryBarrier transitionBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_TRANSFER_WRITE_BIT,
			    VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, src->image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

		{
			VkImageMemoryBarrier transitionBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_TRANSFER_WRITE_BIT,
			    VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, image );

			vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
			    nullptr, 1, &transitionBarrier );
		}

		return RENDER_STATUS_OK;
	} );
}

void* VulkanImageTexture::GetImGuiTextureID()
{
	//
	// Create a descriptor for ImGUI if we do not already have one
	//
	if ( m_imGuiDescriptorSet == VK_NULL_HANDLE )
	{
		m_imGuiDescriptorSet = ImGui_ImplVulkan_AddTexture(
		    m_parent->m_anisoSampler.sampler, imageView, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL );
	}

	return ( void* )m_imGuiDescriptorSet;
}

void VulkanImageTexture::Delete() const
{
	vkDestroyImageView( m_parent->m_device, imageView, nullptr );
	vmaDestroyImage( m_parent->m_allocator, image, allocation );
}

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

	SetDebugName( "VulkanCommandContext Command Pool", VK_OBJECT_TYPE_COMMAND_POOL, ( uint64_t )commandPool );
	SetDebugName( "VulkanCommandContext Command Buffer", VK_OBJECT_TYPE_COMMAND_BUFFER, ( uint64_t )commandBuffer );
	SetDebugName( "VulkanCommandContext Fence", VK_OBJECT_TYPE_FENCE, ( uint64_t )fence );
}

void VulkanCommandContext::Delete() const
{
	if ( m_parent == nullptr )
		return;

	// Wait for the fence to be signaled
	vkWaitForFences( m_parent->m_device, 1, &fence, VK_TRUE, 1000000000 );

	vkDestroyFence( m_parent->m_device, fence, nullptr );
	vkFreeCommandBuffers( m_parent->m_device, commandPool, 1, &commandBuffer );
	vkDestroyCommandPool( m_parent->m_device, commandPool, nullptr );
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

	__debugbreak(); // Invalid / unsupported sampler type.
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

	SetDebugName( bufferInfo.name.c_str(), VK_OBJECT_TYPE_BUFFER, ( uint64_t )buffer );
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

	// Destroy staging buffer
	vmaDestroyBuffer( m_parent->m_allocator, stagingBuffer.buffer, stagingBuffer.allocation );
}

void VulkanBuffer::Delete() const
{
	vmaDestroyBuffer( m_parent->m_allocator, buffer, allocation );
}

// ----------------------------------------------------------------------------------------------------------------------------

std::shared_ptr<VulkanCommandContext> VulkanRenderContext::GetUploadContext( std::thread::id thread )
{
	std::shared_lock lock( m_uploadContextMutex );

	if ( m_uploadContexts.find( thread ) == m_uploadContexts.end() )
	{
		m_uploadContexts[thread] = std::make_shared<VulkanCommandContext>( this );

		vkResetFences( m_device, 1, &m_uploadContexts[thread]->fence );
	}

	return m_uploadContexts[thread];
}

// Create a Vulkan context, set up devices
vkb::Instance VulkanRenderContext::CreateInstanceAndSurface()
{
	vkb::InstanceBuilder builder;
	vkb::Instance vkbInstance;

	auto ret = builder
	               .set_app_name( ClientRoot::GetInstance().m_projectManager->GetProject().name.c_str() ) // Fuck this
	               .set_engine_name( ENGINE_NAME )
	               .request_validation_layers( true )
	               .require_api_version( 1, 3, 0 )
	               .use_default_debug_messenger()
	               .enable_extension( VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME )
	               .enable_extension( VK_EXT_DEBUG_UTILS_EXTENSION_NAME )
	               .build();

	vkbInstance = ret.value();

	m_instance = vkbInstance.instance;
	m_debugMessenger = vkbInstance.debug_messenger;

	volkLoadInstance( m_instance );

	m_window = std::make_unique<Window>( m_parent, 1280, 720 );
	m_surface = m_window->CreateSurface( m_instance );

	return vkbInstance;
}

void VulkanRenderContext::FinalizeAndCreateDevice( vkb::PhysicalDevice physicalDevice )
{
	vkb::DeviceBuilder deviceBuilder( physicalDevice );

	if ( EngineProperties::Raytracing )
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

	SetDebugName( "Main Device", VK_OBJECT_TYPE_DEVICE, ( uint64_t )m_device );
	SetDebugName( "Main Physical Device", VK_OBJECT_TYPE_PHYSICAL_DEVICE, ( uint64_t )m_chosenGPU );

	SetDebugName( "Graphics Queue", VK_OBJECT_TYPE_QUEUE, ( uint64_t )m_graphicsQueue );
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
	if ( EngineProperties::Raytracing )
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

	m_window->m_onWindowResized = [&]( Size2D newSize ) {
		m_swapchain.Update( newSize );
		CreateRenderTargets();
		ClientRoot::GetInstance().m_hostManager->FireEvent( "Event.Window.Resized" );
	};
}

void VulkanRenderContext::CreateCommands()
{
	m_mainContext = VulkanCommandContext( this );
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

void VulkanRenderContext::CreateRenderTargets()
{
	// Are we re-creating render targets? If so, queue the originals for deletion
	if ( m_colorTarget.image != VK_NULL_HANDLE )
	{
		// Make copies of m_colorTarget and m_depthTarget
		VulkanRenderTexture colorTarget( m_colorTarget );
		VulkanRenderTexture depthTarget( m_depthTarget );

		m_frameDeletionQueue.Enqueue( [colorTarget, depthTarget]() {
			// Delete copied render targets
			colorTarget.Delete();
			depthTarget.Delete();
		} );
	}

	Size2D size = m_window->GetWindowSize();

	//
	// Create render targets
	//
	RenderTextureInfo_t renderTextureInfo;
	renderTextureInfo.name = "Main render target";
	renderTextureInfo.width = size.x * renderScale;
	renderTextureInfo.height = size.y * renderScale;

	// BUG: Resizing the window after setting render scale to something high will cause
	// an engine crash.. even though we're limiting sizes?

	//
	// Limit size to something sensible
	// TODO: Query device support?
	//
	const float maxSize = 8192.0f;
	const float aspect = ( float )size.x / ( float )size.y;

	if ( renderTextureInfo.width > maxSize || renderTextureInfo.height > maxSize )
	{
		renderTextureInfo.width = maxSize;
		renderTextureInfo.height = maxSize / aspect;

		spdlog::warn( "Render target size is too large. Clamping to {}x{}", renderTextureInfo.width, renderTextureInfo.height );
	}

	renderTextureInfo.type = RENDER_TEXTURE_DEPTH;
	m_depthTarget = VulkanRenderTexture( this, renderTextureInfo );

	renderTextureInfo.type = RENDER_TEXTURE_COLOR_OPAQUE;
	m_colorTarget = VulkanRenderTexture( this, renderTextureInfo );

	// HACK: Horrific hack to get render textures working with descriptor sets for now
	VulkanImageTexture vkImageTexture;
	vkImageTexture.SetParent( this );
	vkImageTexture.image = m_colorTarget.image;
	vkImageTexture.imageView = m_colorTarget.imageView;
	vkImageTexture.format = m_colorTarget.format;

	m_fullScreenTri.imageTexture = {};
	m_fullScreenTri.imageTexture.m_handle = m_imageTextures.Add( vkImageTexture );
}

void VulkanRenderContext::CreateSamplers()
{
	m_pointSampler = VulkanSampler( this, SAMPLER_TYPE_POINT );
	m_anisoSampler = VulkanSampler( this, SAMPLER_TYPE_ANISOTROPIC );
}

void VulkanRenderContext::CreateImGuiIconFont()
{
	auto& io = ImGui::GetIO();

	ImFontConfig iconConfig = {};
	iconConfig.MergeMode = 1;
	iconConfig.GlyphMinAdvanceX = 16.0f;

	ImWchar iconRanges[] = { ICON_MIN_FA, ICON_MAX_FA, 0 };

	io.Fonts->AddFontFromFileTTF( "content/core/fonts/fa-solid-900.ttf", 12.0f, &iconConfig, iconRanges );
	io.Fonts->AddFontFromFileTTF( "content/core/fonts/fa-regular-400.ttf", 12.0f, &iconConfig, iconRanges );
}

void VulkanRenderContext::CreateImGui()
{
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

#define ADD_FONT( name, path, size )                   \
	name = io.Fonts->AddFontFromFileTTF( path, size ); \
	CreateImGuiIconFont();

	ADD_FONT( m_mainFont, "Content\\core\\fonts\\editor\\Inter-Regular.ttf", 14.0f );
	ADD_FONT( m_boldFont, "Content\\core\\fonts\\editor\\Inter-Bold.ttf", 14.0f );
	ADD_FONT( m_subheadingFont, "Content\\core\\fonts\\editor\\Inter-Medium.ttf", 16.0f );
	ADD_FONT( m_headingFont, "Content\\core\\fonts\\editor\\Inter-Bold.ttf", 24.0f );
	ADD_FONT( m_monospaceFont, "Content\\core\\fonts\\editor\\JetBrainsMono-Regular.ttf", 14.0f );
#undef ADD_FONT

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
	init_info.MinImageCount = 2;
	init_info.ImageCount = m_swapchain.m_swapchainTextures.size();
	init_info.MSAASamples = VK_SAMPLE_COUNT_1_BIT;
	init_info.UseDynamicRendering = true;
	init_info.ColorAttachmentFormat = m_swapchain.m_swapchainTextures[0].format;

	ImGui_ImplVulkan_LoadFunctions(
	    []( const char* function_name, void* user_data ) {
		    return vkGetInstanceProcAddr( ( VkInstance )user_data, function_name );
	    },
	    m_instance );
	ImGui_ImplVulkan_Init( &init_info, nullptr );
	ImmediateSubmit( [&]( VkCommandBuffer cmd ) -> RenderStatus {
		ImGui_ImplVulkan_CreateFontsTexture( cmd );
		return RENDER_STATUS_OK;
	} );
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
}

RenderStatus VulkanRenderContext::BeginImGui()
{
	ImGui_ImplVulkan_NewFrame();
	ImGui_ImplSDL2_NewFrame( m_window->GetSDLWindow() );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::EndImGui()
{
	ImGui::Render();

	if ( ImGui::GetIO().ConfigFlags & ImGuiConfigFlags_ViewportsEnable )
	{
		ImGui::UpdatePlatformWindows();
		ImGui::RenderPlatformWindowsDefault();
	}

	return RENDER_STATUS_OK;
}

void VulkanRenderContext::CreateFullScreenTri()
{
	m_fullScreenTri = {};

	{
		// clang-format off
		std::vector<float> vertices = {
			// Coords			// UVs
			-1.0f, -1.0f, 0.0f,	0.0f, 0.0f,
			3.0f, -1.0f, 0.0f,  2.0f, 0.0f,
			-1.0f, 3.0f, 0.0f,	0.0f, 2.0f
		};
		// clang-format on

		BufferInfo_t bufferInfo = {};
		bufferInfo.name = "Fullscreen triangle vertex buffer";
		bufferInfo.size = sizeof( float ) * vertices.size();
		bufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		bufferInfo.usage = BUFFER_USAGE_FLAG_VERTEX_BUFFER | BUFFER_USAGE_FLAG_TRANSFER_DST;

		m_fullScreenTri.vertexBuffer = VertexBuffer( bufferInfo );

		BufferUploadInfo_t uploadInfo = {};
		uploadInfo.data = {};
		uploadInfo.data.count = vertices.size();
		uploadInfo.data.size = vertices.size() * sizeof( float );
		uploadInfo.data.data = ( void* )vertices.data();

		UploadBuffer( m_fullScreenTri.vertexBuffer.m_handle, uploadInfo );

		m_fullScreenTri.vertexCount = vertices.size();
	}

	{
		std::vector<uint32_t> indices = { 0, 1, 2 };

		BufferInfo_t bufferInfo = {};
		bufferInfo.name = "Fullscreen triangle index buffer";
		bufferInfo.size = sizeof( uint32_t ) * indices.size();
		bufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		bufferInfo.usage = BUFFER_USAGE_FLAG_INDEX_BUFFER | BUFFER_USAGE_FLAG_TRANSFER_DST;

		m_fullScreenTri.indexBuffer = IndexBuffer( bufferInfo );

		BufferUploadInfo_t uploadInfo = {};
		uploadInfo.data = {};
		uploadInfo.data.count = indices.size();
		uploadInfo.data.size = indices.size() * sizeof( uint32_t );
		uploadInfo.data.data = ( void* )indices.data();

		UploadBuffer( m_fullScreenTri.indexBuffer.m_handle, uploadInfo );

		m_fullScreenTri.indexCount = indices.size();
	}

	DescriptorInfo_t descriptorInfo = {};
	DescriptorBindingInfo_t colorTextureBinding = {};

	// HACK: Horrific hack to get render textures working with descriptor sets for now
	VulkanImageTexture vkImageTexture;
	vkImageTexture.SetParent( this );
	vkImageTexture.image = m_colorTarget.image;
	vkImageTexture.imageView = m_colorTarget.imageView;
	vkImageTexture.format = m_colorTarget.format;

	m_fullScreenTri.imageTexture = {};
	m_fullScreenTri.imageTexture.m_handle = m_imageTextures.Add( vkImageTexture );
	colorTextureBinding.texture = &m_fullScreenTri.imageTexture;
	colorTextureBinding.type = DESCRIPTOR_BINDING_TYPE_IMAGE;

	descriptorInfo.name = "Fullscreen triangle descriptor";
	descriptorInfo.bindings = std::vector<DescriptorBindingInfo_t>{ colorTextureBinding };

	m_fullScreenTri.descriptor = Descriptor( descriptorInfo );

	VertexAttributeInfo_t positionAttribute = {};
	positionAttribute.format = VERTEX_ATTRIBUTE_FORMAT_FLOAT3;
	positionAttribute.name = "Position";

	VertexAttributeInfo_t uvAttribute = {};
	uvAttribute.format = VERTEX_ATTRIBUTE_FORMAT_FLOAT2;
	uvAttribute.name = "UV";

	PipelineInfo_t pipelineInfo = {};
	pipelineInfo.ignoreDepth = true;
	pipelineInfo.renderToSwapchain = true;
	pipelineInfo.descriptors = std::vector<Descriptor*>{ &m_fullScreenTri.descriptor };
	pipelineInfo.vertexAttributes = std::vector<VertexAttributeInfo_t>{ positionAttribute, uvAttribute };
	pipelineInfo.shaderInfo = {};

	// Compile fragment & vertex shaders
	std::vector<unsigned int> vertexShaderBits;
	std::vector<unsigned int> fragmentShaderBits;

	// Vertex
	{
		if ( !ShaderCompiler::Instance().Compile( SHADER_TYPE_VERTEX, g_fullScreenTriVertexShader.c_str(), vertexShaderBits ) )
		{
			ErrorMessage( "Fullscreen triangle vertex shader failed to compile." );
			abort();
		}

		pipelineInfo.shaderInfo.vertexShaderData = vertexShaderBits;
	}

	// Fragment
	{
		if ( !ShaderCompiler::Instance().Compile(
		         SHADER_TYPE_FRAGMENT, g_fullScreenTriFragmentShader.c_str(), fragmentShaderBits ) )
		{
			ErrorMessage( "Fullscreen triangle fragment shader failed to compile." );
			abort();
		}

		pipelineInfo.shaderInfo.fragmentShaderData = fragmentShaderBits;
	}

	m_fullScreenTri.pipeline = Pipeline( pipelineInfo );
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

		m_hasInitialized = true;

		// Resources
		CreateSamplers();
		CreateSwapchain();
		CreateCommands();
		CreateSyncStructures();
		CreateDescriptors();
		CreateImGui();
		CreateRenderTargets();
		CreateFullScreenTri();
	}

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::Shutdown()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	//
	// Delete everything
	//
	ImGui_ImplVulkan_Shutdown();
	ImGui_ImplSDL2_Shutdown();
	ImGui::DestroyContext();

	// Delete command contexts
	m_mainContext.Delete();
	for ( auto& context : m_uploadContexts )
	{
		context.second->Delete();
	}

	// Delete allocated objects
	// Must be done in a specific order
	m_pipelines.ForEach( []( std::shared_ptr<VulkanPipeline> pipeline ) { pipeline->Delete(); } );
	m_descriptors.ForEach( []( std::shared_ptr<VulkanDescriptor> descriptor ) { descriptor->Delete(); } );
	m_shaders.ForEach( []( std::shared_ptr<VulkanShader> shader ) { shader->Delete(); } );
	m_buffers.ForEach( []( std::shared_ptr<VulkanBuffer> buffer ) { buffer->Delete(); } );
	m_imageTextures.ForEach( []( std::shared_ptr<VulkanImageTexture> imageTexture ) { imageTexture->Delete(); } );
	m_renderTextures.ForEach( []( std::shared_ptr<VulkanRenderTexture> renderTexture ) { renderTexture->Delete(); } );

	m_depthTarget.Delete();
	m_colorTarget.Delete();

	// Delete main swapchain
	m_swapchain.Delete();

	// Delete raw vulkan objects (eg. device, instance, descriptor pool, semaphores, debugm essenger)
	// Must also be done in a specific order
	vkDestroyDebugUtilsMessengerEXT( m_instance, m_debugMessenger, nullptr );
	vkDestroySemaphore( m_device, m_presentSemaphore, nullptr );
	vkDestroySemaphore( m_device, m_renderSemaphore, nullptr );

	vkDestroyDescriptorPool( m_device, m_descriptorPool, nullptr );

	// Finally, destroy the allocator
	vmaDestroyAllocator( m_allocator );

	vkDestroyDevice( m_device, nullptr );
	vkDestroyInstance( m_instance, nullptr );

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

void VulkanRenderContext::RenderImGui()
{
	VkCommandBuffer cmd = m_mainContext.commandBuffer;

	if ( m_isRenderPassActive )
	{
		vkCmdEndRendering( cmd );
		m_isRenderPassActive = false;
	}

	// Draw UI
	VkRenderingAttachmentInfo uiAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_swapchainTarget.imageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	uiAttachmentInfo.loadOp = VK_ATTACHMENT_LOAD_OP_LOAD; // Preserve existing color data (3d scene)

	VkRenderingInfo imguiRenderInfo = VKInit::RenderingInfo( &uiAttachmentInfo, nullptr, m_window->GetWindowSize() );

	vkCmdBeginRendering( cmd, &imguiRenderInfo );
	ImGui_ImplVulkan_RenderDrawData( ImGui::GetDrawData(), cmd );
	vkCmdEndRendering( cmd );
}

RenderStatus VulkanRenderContext::BeginRendering()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	// Show window now that we're done setting up
	m_window->Show();

	// Render scale change checking
	{
		if ( lastRenderScale != renderScale )
		{
			// Render scale has changed - re-create render targets
			CreateRenderTargets();
		}

		lastRenderScale = renderScale;
	}

	Size2D renderSize = m_colorTarget.size;

	if ( !CanRender() )
	{
		return RENDER_STATUS_WINDOW_SIZE_INVALID;
	}

	// Wait until we can render ( 1 second timeout )
	VK_CHECK( vkWaitForFences( m_device, 1, &m_mainContext.fence, true, 1000000000 ) );
	VK_CHECK( vkResetFences( m_device, 1, &m_mainContext.fence ) );

	// Acquire swapchain image ( 1 second timeout )
	m_swapchainImageIndex = m_swapchain.AcquireSwapchainImageIndex( m_device, m_presentSemaphore, m_mainContext );
	m_swapchainTarget = m_swapchain.m_swapchainTextures[m_swapchainImageIndex];

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
	viewport.width = static_cast<float>( renderSize.x );
	viewport.height = static_cast<float>( renderSize.y );

	VkRect2D scissor = { { 0, 0 }, { renderSize.x, renderSize.y } };
	vkCmdSetScissor( cmd, 0, 1, &scissor );
	vkCmdSetViewport( cmd, 0, 1, &viewport );

	//
	// We want to draw the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL before presenting
	//
	VkImageMemoryBarrier writeToColorTargetBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_SHADER_READ_BIT,
	    VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, m_colorTarget.image );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, 0, 0,
	    nullptr, 0, nullptr, 1, &writeToColorTargetBarrier );

	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkClearValue depthClear = {};
	depthClear.depthStencil.depth = 1.0f;

	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_colorTarget.imageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	colorAttachmentInfo.clearValue = colorClear;

	VkRenderingAttachmentInfo depthAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_depthTarget.imageView, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL );
	depthAttachmentInfo.clearValue = depthClear;

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( &colorAttachmentInfo, &depthAttachmentInfo, renderSize );
	vkCmdBeginRendering( cmd, &renderInfo );

	m_isRenderPassActive = true;
	m_renderingActive = true;
	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::EndRendering()
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	VkCommandBuffer cmd = m_mainContext.commandBuffer;

	if ( m_isRenderPassActive )
	{
		vkCmdEndRendering( cmd );
		m_isRenderPassActive = false;
	}

	//
	// We want to draw the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL before presenting
	//
	VkImageMemoryBarrier startRenderBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
	    VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, m_swapchainTarget.image );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, 0, 0, nullptr,
	    0, nullptr, 1, &startRenderBarrier );

	VkImageMemoryBarrier readFromColorTargetBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
	    VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, m_colorTarget.image );

	vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0,
	    nullptr, 0, nullptr, 1, &readFromColorTargetBarrier );

	//
	// Set viewport & scissor
	//
	Size2D windowSize = m_window->GetWindowSize();
	VkViewport viewport = {};
	viewport.minDepth = 0.0;
	viewport.maxDepth = 1.0;
	viewport.width = static_cast<float>( windowSize.x );
	viewport.height = static_cast<float>( windowSize.y );

	VkRect2D scissor = { { 0, 0 }, { windowSize.x, windowSize.y } };
	vkCmdSetScissor( cmd, 0, 1, &scissor );
	vkCmdSetViewport( cmd, 0, 1, &viewport );

	//
	// Render fullscreen tri to screen
	//
	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_swapchainTarget.imageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	colorAttachmentInfo.clearValue = colorClear;

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( &colorAttachmentInfo, nullptr, windowSize );

	vkCmdBeginRendering( cmd, &renderInfo );

	BindVertexBuffer( m_fullScreenTri.vertexBuffer );
	BindIndexBuffer( m_fullScreenTri.indexBuffer );
	BindPipeline( m_fullScreenTri.pipeline );
	BindDescriptor( m_fullScreenTri.descriptor );

	DescriptorUpdateInfo_t updateInfo = {};
	updateInfo.binding = 0;
	updateInfo.samplerType = SAMPLER_TYPE_POINT;
	updateInfo.src = &m_fullScreenTri.imageTexture;

	UpdateDescriptor( m_fullScreenTri.descriptor, updateInfo );

	Draw( m_fullScreenTri.vertexCount, m_fullScreenTri.indexCount, 1 );

	vkCmdEndRendering( cmd );

	//
	// Render editor
	//
	RenderImGui();

	//
	// We want to present the image, so we'll manually transition the layout to
	// VK_IMAGE_LAYOUT_PRESENT_SRC_KHR before presenting
	//
	VkImageMemoryBarrier endRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
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
		return RENDER_STATUS_WINDOW_SIZE_INVALID;
	}

	VK_CHECK( vkQueuePresentKHR( m_graphicsQueue, &presentInfo ) );

	//
	// Delete everything in the deletion queue, because we can be 100% sure we're not using it here
	//
	m_frameDeletionQueue.Flush();

	m_renderingActive = false;
	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindPipeline( Pipeline p )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	std::shared_ptr<VulkanPipeline> pipeline = m_pipelines.Get( p.m_handle );

	m_pipeline = pipeline;

	vkCmdBindPipeline( m_mainContext.commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipeline->pipeline );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindDescriptor( Descriptor d )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	std::shared_ptr<VulkanDescriptor> descriptor = m_descriptors.Get( d.m_handle );

	vkCmdBindDescriptorSets( m_mainContext.commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipeline->layout, 0, 1,
	    &descriptor->descriptorSet, 0, nullptr );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::UpdateDescriptor( Descriptor d, DescriptorUpdateInfo_t updateInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	BindDescriptor( d );

	std::shared_ptr<VulkanDescriptor> descriptor = m_descriptors.Get( d.m_handle );
	std::shared_ptr<VulkanImageTexture> texture = m_imageTextures.Get( updateInfo.src->m_handle );

	VkDescriptorImageInfo imageInfo = {};
	imageInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
	imageInfo.imageView = texture->imageView;
	imageInfo.sampler = SAMPLER_TYPE_ANISOTROPIC ? m_anisoSampler.sampler : m_pointSampler.sampler; // TODO

	auto descriptorWrite = VKInit::WriteDescriptorImage(
	    VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, descriptor->descriptorSet, &imageInfo, updateInfo.binding );

	vkUpdateDescriptorSets( m_device, 1, &descriptorWrite, 0, nullptr );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindVertexBuffer( VertexBuffer vb )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	std::shared_ptr<VulkanBuffer> vertexBuffer = m_buffers.Get( vb.m_handle );

	VkDeviceSize offset = 0;
	vkCmdBindVertexBuffers( m_mainContext.commandBuffer, 0, 1, &vertexBuffer->buffer, &offset );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::BindIndexBuffer( IndexBuffer ib )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );
	ErrorIf( !m_renderingActive, RENDER_STATUS_BEGIN_END_MISMATCH );

	std::shared_ptr<VulkanBuffer> indexBuffer = m_buffers.Get( ib.m_handle );

	VkDeviceSize offset = 0;
	vkCmdBindIndexBuffer( m_mainContext.commandBuffer, indexBuffer->buffer, offset, VK_INDEX_TYPE_UINT32 );

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

	if ( m_isRenderPassActive )
	{
		vkCmdEndRendering( m_mainContext.commandBuffer );
	}

	std::shared_ptr<VulkanRenderTexture> renderTexture = m_renderTextures.Get( rt.m_handle );

	// Transition to VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
	VkImageMemoryBarrier startRenderImageMemoryBarrier = VKInit::ImageMemoryBarrier( VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT,
	    VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, renderTexture->image );

	vkCmdPipelineBarrier( m_mainContext.commandBuffer, VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT,
	    VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, 0, 0, nullptr, 0, nullptr, 1, &startRenderImageMemoryBarrier );

	VkClearValue colorClear = { { { 0.0f, 0.0f, 0.0f, 1.0f } } };
	VkClearValue depthClear = {};
	depthClear.depthStencil.depth = 1.0f;

	VkRenderingAttachmentInfo colorAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( renderTexture->imageView, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL );
	colorAttachmentInfo.clearValue = colorClear;

	VkRenderingAttachmentInfo depthAttachmentInfo =
	    VKInit::RenderingAttachmentInfo( m_depthTarget.imageView, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL );
	depthAttachmentInfo.clearValue = depthClear;

	VkRenderingInfo renderInfo = VKInit::RenderingInfo( &colorAttachmentInfo, &depthAttachmentInfo, renderTexture->size );
	vkCmdBeginRendering( m_mainContext.commandBuffer, &renderInfo );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::GetRenderSize( Size2D* outSize )
{
	*outSize = m_colorTarget.size;

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::GetWindowSize( Size2D* outSize )
{
	*outSize = m_window->GetWindowSize();

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

	std::shared_ptr<VulkanImageTexture> imageTexture = m_imageTextures.Get( handle );

	if ( imageTexture == nullptr )
		return RENDER_STATUS_INVALID_HANDLE;

	imageTexture->SetData( pipelineInfo );

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	std::shared_ptr<VulkanImageTexture> imageTexture = m_imageTextures.Get( handle );

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

	std::shared_ptr<VulkanBuffer> buffer = m_buffers.Get( handle );

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

RenderStatus VulkanRenderContext::GetGPUInfo( GPUInfo* outInfo )
{
	GPUInfo info = {};
	info.gpuName = m_deviceProperties.deviceName;

	*outInfo = info;

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::GetImGuiTextureID( ImageTexture* texture, void** outTextureId )
{
	std::shared_ptr<VulkanImageTexture> vkTexture = m_imageTextures.Get( texture->m_handle );
	*outTextureId = vkTexture->GetImGuiTextureID();

	return RENDER_STATUS_OK;
}

RenderStatus VulkanRenderContext::ImmediateSubmit( std::function<RenderStatus( VkCommandBuffer commandBuffer )> func )
{
	ErrorIf( !m_hasInitialized, RENDER_STATUS_NOT_INITIALIZED );

	RenderStatus status;

	// Get a command context for this thread, prevents threading issues
	std::thread::id threadId = std::this_thread::get_id();
	std::shared_ptr<VulkanCommandContext> currentContext = GetUploadContext( threadId );

	VkCommandBuffer cmd = currentContext->commandBuffer;
	VkCommandBufferBeginInfo cmdBeginInfo = VKInit::CommandBufferBeginInfo( VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT );
	VK_CHECK( vkBeginCommandBuffer( cmd, &cmdBeginInfo ) );

	status = func( cmd );

	VK_CHECK( vkEndCommandBuffer( cmd ) );

	VkSubmitInfo submit = VKInit::SubmitInfo( &cmd );
	VK_CHECK( vkQueueSubmit( m_graphicsQueue, 1, &submit, currentContext->fence ) );

	vkWaitForFences( m_device, 1, &currentContext->fence, true, 9999999999 );
	vkResetFences( m_device, 1, &currentContext->fence );

	vkResetCommandPool( m_device, currentContext->commandPool, 0 );

	return status;
}

// ----------------------------------------------------------------------------------------------------------------------------

RenderStatus VulkanShader::LoadShaderModule(
    std::vector<uint32_t> shaderData, ShaderType shaderType, VkShaderModule* outShaderModule )
{
	VkDevice device = m_parent->m_device;

	VkShaderModuleCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.pNext = nullptr;

	createInfo.codeSize = shaderData.size() * sizeof( uint32_t );
	createInfo.pCode = shaderData.data();

	VkShaderModule shaderModule;

	if ( vkCreateShaderModule( device, &createInfo, nullptr, &shaderModule ) != VK_SUCCESS )
	{
		spdlog::error( "Could not compile shader" );
		return RENDER_STATUS_SHADER_COMPILE_FAILED;
	}

	*outShaderModule = shaderModule;

	return RENDER_STATUS_OK;
}

VulkanShader::VulkanShader( VulkanRenderContext* parent, ShaderInfo_t shaderInfo )
{
	SetParent( parent );

	if ( LoadShaderModule( shaderInfo.fragmentShaderData, SHADER_TYPE_FRAGMENT, &fragmentShader ) != RENDER_STATUS_OK )
		spdlog::error( "VulkanShader::VulkanShader: Fragment shader failed to compile" );

	if ( LoadShaderModule( shaderInfo.vertexShaderData, SHADER_TYPE_VERTEX, &vertexShader ) != RENDER_STATUS_OK )
		spdlog::error( "VulkanShader::VulkanShader: Vertex shader failed to compile" );

	SetDebugName( shaderInfo.name.c_str(), VK_OBJECT_TYPE_SHADER_MODULE, ( uint64_t )fragmentShader );
	SetDebugName( shaderInfo.name.c_str(), VK_OBJECT_TYPE_SHADER_MODULE, ( uint64_t )vertexShader );
}

void VulkanShader::Delete() const
{
	vkDestroyShaderModule( m_parent->m_device, fragmentShader, nullptr );
	vkDestroyShaderModule( m_parent->m_device, vertexShader, nullptr );
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

	VkDescriptorSetLayoutCreateInfo layoutInfo =
	    VKInit::DescriptorSetLayoutCreateInfo( bindings.data(), static_cast<uint32_t>( bindings.size() ) );
	VK_CHECK( vkCreateDescriptorSetLayout( m_parent->m_device, &layoutInfo, nullptr, &descriptorSetLayout ) );

	VkDescriptorSetAllocateInfo allocInfo =
	    VKInit::DescriptorSetAllocateInfo( m_parent->m_descriptorPool, &descriptorSetLayout, 1 );

	VK_CHECK( vkAllocateDescriptorSets( m_parent->m_device, &allocInfo, &descriptorSet ) );

	SetDebugName( descriptorInfo.name.c_str(), VK_OBJECT_TYPE_DESCRIPTOR_SET, ( uint64_t )descriptorSet );

	std::string descriptorSetName = descriptorInfo.name + " Set";
	SetDebugName( descriptorSetName.c_str(), VK_OBJECT_TYPE_DESCRIPTOR_SET_LAYOUT, ( uint64_t )descriptorSetLayout );
}

VkDescriptorType VulkanDescriptor::GetDescriptorType( DescriptorBindingType type )
{
	switch ( type )
	{
	case DESCRIPTOR_BINDING_TYPE_IMAGE:
		return VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
	}

	__debugbreak(); // Invalid / unsupported descriptor binding type
}

void VulkanDescriptor::Delete() const
{
	vkDestroyDescriptorSetLayout( m_parent->m_device, descriptorSetLayout, nullptr );
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
		std::shared_ptr<VulkanDescriptor> vkDescriptor = m_parent->m_descriptors.Get( descriptor->m_handle );

		setLayouts.push_back( vkDescriptor->descriptorSetLayout );
	}

	pipeline_layout_info.pSetLayouts = setLayouts.data();
	pipeline_layout_info.setLayoutCount = static_cast<uint32_t>( setLayouts.size() );

	VK_CHECK( vkCreatePipelineLayout( m_parent->m_device, &pipeline_layout_info, nullptr, &builder.m_pipelineLayout ) );

	layout = builder.m_pipelineLayout;

	std::string pipelineLayoutName = pipelineInfo.name + " Layout";
	SetDebugName( pipelineLayoutName.c_str(), VK_OBJECT_TYPE_PIPELINE_LAYOUT, ( uint64_t )layout );

	builder.m_rasterizer = VKInit::PipelineRasterizationStateCreateInfo( VK_POLYGON_MODE_FILL );
	builder.m_multisampling = VKInit::PipelineMultisampleStateCreateInfo();
	builder.m_colorBlendAttachment = VKInit::PipelineColorBlendAttachmentState();

	VulkanShader shader( m_parent, pipelineInfo.shaderInfo );

	std::vector<VkPipelineShaderStageCreateInfo> shaderStages;

	shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_FRAGMENT_BIT, shader.fragmentShader ) );
	shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_VERTEX_BIT, shader.vertexShader ) );

	builder.m_shaderStages = shaderStages;

	// Calculate stride size
	uint32_t stride = 0;
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

	uint32_t offset = 0;

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

	if ( pipelineInfo.renderToSwapchain )
	{
		pipeline = builder.Build( m_parent->m_device, m_parent->m_colorTarget.format, VK_FORMAT_D32_SFLOAT_S8_UINT );
	}
	else
	{
		pipeline = builder.Build( m_parent->m_device, m_parent->m_colorTarget.format, m_parent->m_depthTarget.format );
	}

	SetDebugName( pipelineInfo.name.c_str(), VK_OBJECT_TYPE_PIPELINE, ( uint64_t )pipeline );
}

void VulkanPipeline::Delete() const
{
	vkDestroyPipeline( m_parent->m_device, pipeline, nullptr );
	vkDestroyPipelineLayout( m_parent->m_device, layout, nullptr );
}
