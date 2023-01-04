#pragma once
#include <rendering.h>
#include <util.h>

//@InteropGen generate class
class Texture
{
public:
	ImageTexture m_image;

	//@InteropGen ignore
	Texture(){};

	Texture( uint32_t width, uint32_t height );
	void SetData( uint32_t width, uint32_t height, uint32_t mipCount, UtilArray mipData, int imageFormat );
	void Copy( uint32_t srcX, uint32_t srcY, uint32_t dstX, uint32_t dstY, uint32_t width, uint32_t height, Texture* src );
};