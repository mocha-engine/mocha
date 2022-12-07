#include "texture.h"

#include <globalvars.h>
#include <vulkan/rendermanager.h>
#include <vulkan/vkinit.h>

void Texture::SetData( uint32_t width, uint32_t height, void* data, VkFormat imageFormat )
{
	VkDeviceSize imageSize = width * height * 4;

	AllocatedBuffer stagingBuffer =
	    g_renderManager->CreateBuffer( imageSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VMA_MEMORY_USAGE_CPU_ONLY );

	void* mappedData;
	vmaMapMemory( g_renderManager->m_allocator, stagingBuffer.allocation, &mappedData );
	memcpy( mappedData, data, static_cast<size_t>( imageSize ) );
	vmaUnmapMemory( g_renderManager->m_allocator, stagingBuffer.allocation );

	VkExtent3D imageExtent;
	imageExtent.width = width;
	imageExtent.height = height;
	imageExtent.depth = 1;

	VkImageCreateInfo imageCreateInfo =
	    VKInit::ImageCreateInfo( imageFormat, VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT, imageExtent );
	
	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;

	vmaCreateImage( g_renderManager->m_allocator, &imageCreateInfo, &allocInfo, &image.image, &image.allocation, nullptr );

	g_renderManager->ImmediateSubmit( [&]( VkCommandBuffer cmd ) {
		VkImageSubresourceRange range;
		range.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		range.baseMipLevel = 0;
		range.levelCount = 1;
		range.baseArrayLayer = 0;
		range.layerCount = 1;

		VkImageMemoryBarrier imageBarrier_toTransfer = {};
		imageBarrier_toTransfer.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;

		imageBarrier_toTransfer.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		imageBarrier_toTransfer.newLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toTransfer.image = image.image;
		imageBarrier_toTransfer.subresourceRange = range;

		imageBarrier_toTransfer.srcAccessMask = 0;
		imageBarrier_toTransfer.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		
		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0, nullptr,
		    1, &imageBarrier_toTransfer );

		VkBufferImageCopy copyRegion = {};
		copyRegion.bufferOffset = 0;
		copyRegion.bufferRowLength = 0;
		copyRegion.bufferImageHeight = 0;

		copyRegion.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		copyRegion.imageSubresource.mipLevel = 0;
		copyRegion.imageSubresource.baseArrayLayer = 0;
		copyRegion.imageSubresource.layerCount = 1;
		copyRegion.imageExtent = imageExtent;
		
		vkCmdCopyBufferToImage(
		    cmd, stagingBuffer.buffer, image.image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &copyRegion );

		VkImageMemoryBarrier imageBarrier_toReadable = imageBarrier_toTransfer;

		imageBarrier_toReadable.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toReadable.newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;

		imageBarrier_toReadable.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		imageBarrier_toReadable.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		
		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
		    nullptr, 1, &imageBarrier_toReadable );
	} );

	VkImageViewCreateInfo imageViewInfo = VKInit::ImageViewCreateInfo( imageFormat, image.image, VK_IMAGE_ASPECT_COLOR_BIT );
	vkCreateImageView( g_renderManager->m_device, &imageViewInfo, nullptr, &imageView );

	spdlog::info( "Created texture with size {}x{}", width, height );
}
