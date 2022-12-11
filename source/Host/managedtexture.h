#pragma once

#include "game/model.h"
#include <texture.h>

class RenderManager;

//@InteropGen generate class
class ManagedTexture
{
private:
	Texture m_texture;

public:
	void SetMipData( uint32_t width, uint32_t height, uint32_t mipCount, uint32_t dataSize, void* data, int format );

	//@InteropGen ignore
	inline Texture GetTexture() { return m_texture; }
};