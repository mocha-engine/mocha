#pragma once
#include <rendering.h>
#include <util.h>

class Texture
{
public:
	ImageTexture m_image;
	Size2D m_size;

	Texture(){};

	GENERATE_BINDINGS Texture( uint32_t width, uint32_t height );
	GENERATE_BINDINGS void SetData( uint32_t width, uint32_t height, uint32_t mipCount, UtilArray mipData, int imageFormat );
	GENERATE_BINDINGS void Copy( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );
};