#pragma once
#include <algorithm>
#include <managedtypes.h>
#include <spdlog/spdlog.h>
#include <vk_types.h>

//@InteropGen generate class
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
	void SetData( uint32_t width, uint32_t height, uint32_t mipCount, InteropStruct mipData, int imageFormat );

	//@InteropGen ignore
	inline AllocatedImage GetImage() { return m_image; }

	//@InteropGen ignore
	inline VkImageView GetImageView() { return m_imageView; }
};