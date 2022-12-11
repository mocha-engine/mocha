#pragma once
#include <vulkan/types.h>

class Texture
{
private:
	AllocatedImage image;
	VkImageView imageView;

public:
	void SetData( uint32_t width, uint32_t height, void* data, VkFormat imageFormat );

	inline AllocatedImage GetImage() { return image; }
	inline VkImageView GetImageView() { return imageView; }
};