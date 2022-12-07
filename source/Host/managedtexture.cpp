#include "managedtexture.h"

void ManagedTexture::SetData( uint32_t width, uint32_t height, void* data, int format )
{
	spdlog::info( "ManagedTexture: Image {}x{}", width, height );
	
	m_texture.SetData( width, height, data, (VkFormat)format );
}