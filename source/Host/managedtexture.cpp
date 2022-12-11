#include "managedtexture.h"

void ManagedTexture::SetMipData( uint32_t width, uint32_t height, uint32_t mipCount, uint32_t dataSize, void* data, int format )
{
	spdlog::info( "ManagedTexture: Image {}x{}", width, height );
	
	m_texture.SetMipData( width, height, mipCount, dataSize, data, (VkFormat)format );
}