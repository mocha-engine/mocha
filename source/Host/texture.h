#pragma once
#include <algorithm>
#include <spdlog/spdlog.h>
#include <vulkan/types.h>

class Texture
{
private:
	AllocatedImage m_image;
	VkImageView m_imageView;

	inline void CalcMipSize( uint32_t inWidth, uint32_t inHeight, uint32_t mipLevel, uint32_t* outWidth, uint32_t* outHeight )
	{
		*outWidth = inWidth >> mipLevel;
		*outHeight = inHeight >> mipLevel;
	}

public:
	void SetMipData( uint32_t width, uint32_t height, uint32_t mipCount, uint32_t dataSize, void* data, VkFormat imageFormat );

	inline AllocatedImage GetImage() { return m_image; }
	inline VkImageView GetImageView() { return m_imageView; }
};