#include "texture.h"

#include <Misc/globalvars.h>
#include <Rendering/Platform/Vulkan/vkinit.h>
#include <Rendering/rendermanager.h>
#include <Root/root.h>

Texture::Texture( Root* root, const char* name, uint32_t width, uint32_t height )
{
	m_parent = root;

	ImageTextureInfo_t info = {};
	info.name = std::string( name );
	info.width = width;
	info.height = height;

	m_size = { width, height };
	m_image = ImageTexture( m_parent, info );
}

void Texture::SetData( uint32_t width, uint32_t height, uint32_t mipCount, UtilArray mipData, int _imageFormat )
{
	TextureData_t textureData = {};
	textureData.width = width;
	textureData.height = height;
	textureData.mipCount = mipCount;
	textureData.mipData = mipData;
	textureData.imageFormat = _imageFormat;

	m_image.SetData( textureData );
}

void Texture::Copy( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src )
{
	TextureCopyData_t copyData = {};
	copyData.srcX = srcX;
	copyData.srcY = srcY;
	copyData.dstX = dstX;
	copyData.dstY = dstY;
	copyData.width = width;
	copyData.height = height;
	copyData.src = &src->m_image;

	m_image.Copy( copyData );
}