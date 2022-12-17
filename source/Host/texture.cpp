#include "texture.h"

#include <globalvars.h>
#include <rendermanager.h>
#include <vkinit.h>

void Texture::SetData( uint32_t width, uint32_t height, uint32_t mipCount, InteropArray mipData, int imageFormat )
{
	VkDeviceSize imageSize = mipData.size;

	AllocatedBuffer stagingBuffer =
	    g_renderManager->CreateBuffer( imageSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VMA_MEMORY_USAGE_CPU_ONLY );

	void* mappedData;
	vmaMapMemory( g_renderManager->m_allocator, stagingBuffer.allocation, &mappedData );
	memcpy( mappedData, mipData.data, static_cast<size_t>( imageSize ) );
	vmaUnmapMemory( g_renderManager->m_allocator, stagingBuffer.allocation );

	VkExtent3D imageExtent;
	imageExtent.width = width;
	imageExtent.height = height;
	imageExtent.depth = 1;

	VkImageCreateInfo imageCreateInfo = VKInit::ImageCreateInfo(
	    ( VkFormat )imageFormat, VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_TRANSFER_DST_BIT, imageExtent, mipCount );

	VmaAllocationCreateInfo allocInfo = {};
	allocInfo.usage = VMA_MEMORY_USAGE_AUTO;

	vmaCreateImage( g_renderManager->m_allocator, &imageCreateInfo, &allocInfo, &m_image.image, &m_image.allocation, nullptr );

	g_renderManager->ImmediateSubmit( [&]( VkCommandBuffer cmd ) {
		VkImageSubresourceRange range;
		range.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		range.baseMipLevel = 0;
		range.levelCount = mipCount;
		range.baseArrayLayer = 0;
		range.layerCount = 1;

		VkImageMemoryBarrier imageBarrier_toTransfer = {};
		imageBarrier_toTransfer.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;

		imageBarrier_toTransfer.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		imageBarrier_toTransfer.newLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toTransfer.image = m_image.image;
		imageBarrier_toTransfer.subresourceRange = range;

		imageBarrier_toTransfer.srcAccessMask = 0;
		imageBarrier_toTransfer.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;

		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT, VK_PIPELINE_STAGE_TRANSFER_BIT, 0, 0, nullptr, 0, nullptr,
		    1, &imageBarrier_toTransfer );

		std::vector<VkBufferImageCopy> mipRegions = {};

		for ( size_t mip = 0; mip < mipCount; mip++ )
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
				CalcMipSize( width, height, i, &mipWidth, &mipHeight );
				bufferOffset += mipWidth * mipHeight;
			}

			spdlog::trace( "Offset for mip {} on texture size {}x{} is {}", mip, width, height, bufferOffset );

			VkExtent3D mipExtent;
			CalcMipSize( width, height, mip, &mipExtent.width, &mipExtent.height );
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

		vkCmdCopyBufferToImage( cmd, stagingBuffer.buffer, m_image.image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
		    mipRegions.size(), mipRegions.data() );

		VkImageMemoryBarrier imageBarrier_toReadable = imageBarrier_toTransfer;

		imageBarrier_toReadable.oldLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		imageBarrier_toReadable.newLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;

		imageBarrier_toReadable.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		imageBarrier_toReadable.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;

		vkCmdPipelineBarrier( cmd, VK_PIPELINE_STAGE_TRANSFER_BIT, VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT, 0, 0, nullptr, 0,
		    nullptr, 1, &imageBarrier_toReadable );
	} );

	VkImageViewCreateInfo imageViewInfo =
	    VKInit::ImageViewCreateInfo( ( VkFormat )imageFormat, m_image.image, VK_IMAGE_ASPECT_COLOR_BIT, mipCount );
	vkCreateImageView( g_renderManager->m_device, &imageViewInfo, nullptr, &m_imageView );

	spdlog::info( "Created texture with size {}x{}", width, height );
}
