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
	void SetData( uint32_t width, uint32_t height, uint32_t mipCount, InteropArray mipData, int imageFormat );
	void Copy( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );
	void Blit( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );

	//@InteropGen ignore
	inline AllocatedImage GetImage() { return m_image; }

	//@InteropGen ignore
	inline VkImageView GetImageView() { return m_imageView; }
};