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
	void SetData( uint32_t width, uint32_t height, void* data, int format );

	//@InteropGen ignore
	inline Texture GetTexture() { return m_texture; }
};