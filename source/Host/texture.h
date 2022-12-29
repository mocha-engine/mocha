#pragma once
#include <algorithm>
#include <backends/imgui_impl_vulkan.h>
#include <managedtypes.h>
#include <spdlog/spdlog.h>
#include <vk_types.h>

//@InteropGen generate class
class Texture
{
private:
	AllocatedImage m_image;
	VkImageView m_imageView;

	VkDescriptorSet m_imGuiDescriptorSet;

	inline int GetTexelBlockSize( VkFormat format )
	{
		switch ( format )
		{
		case VkFormat::VK_FORMAT_R8G8B8A8_SRGB:
		case VkFormat::VK_FORMAT_R8G8B8A8_UNORM:
			return 4;
			break;
		case VkFormat::VK_FORMAT_BC3_SRGB_BLOCK:
		case VkFormat::VK_FORMAT_BC3_UNORM_BLOCK:
			return 1;
			break;
		case VkFormat::VK_FORMAT_BC5_UNORM_BLOCK:
		case VkFormat::VK_FORMAT_BC5_SNORM_BLOCK:
			return 1;
			break;
		}

		assert( false && "Format is not supported." ); // Format is not supported
		return -1;
	}

	inline void GetMipDimensions(
	    uint32_t inWidth, uint32_t inHeight, uint32_t mipLevel, uint32_t* outWidth, uint32_t* outHeight )
	{
		*outWidth = inWidth >> mipLevel;
		*outHeight = inHeight >> mipLevel;
	}

	inline int CalcMipSize( uint32_t inWidth, uint32_t inHeight, uint32_t mipLevel, VkFormat format )
	{
		uint32_t outWidth, outHeight;
		GetMipDimensions( inWidth, inHeight, mipLevel, &outWidth, &outHeight );
		return outWidth * outHeight * GetTexelBlockSize( format );
	}

public:
	void SetData( uint32_t width, uint32_t height, uint32_t mipCount, InteropArray mipData, int imageFormat );
	void Copy( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );
	void Blit( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );

	//@InteropGen ignore
	inline AllocatedImage GetImage() { return m_image; }

	//@InteropGen ignore
	inline VkImageView GetImageView() { return m_imageView; }

	//@InteropGen ignore
	ImTextureID GetImGuiID();
};