#include "baserendercontext.h"

#include <Root/clientroot.h>

float lastRenderScale = -1.0f;

// TODO: Cvar hooks so that we can change things when cvars change (i.e. re-create render targets when
// render.scale is changed)
FloatCVar renderScale( "render.scale", 1.0f, CVarFlags::Archive, "Multiplier for render resolution scaling" );

// ----------------------------------------------------------------------------------------------------

ImageTexture::ImageTexture( ImageTextureInfo_t info )
{
	Globals::m_renderContext->CreateImageTexture( info, &m_handle );
}

void ImageTexture::SetData( TextureData_t textureData )
{
	Globals::m_renderContext->SetImageTextureData( m_handle, textureData );
}

void ImageTexture::Copy( TextureCopyData_t copyData )
{
	Globals::m_renderContext->CopyImageTexture( m_handle, copyData );
}

// ----------------------------------------------------------------------------------------------------

BaseBuffer::BaseBuffer( BufferInfo_t info )
{
	Globals::m_renderContext->CreateBuffer( info, &m_handle );
}

void BaseBuffer::Upload( BufferUploadInfo_t uploadInfo )
{
	Globals::m_renderContext->UploadBuffer( m_handle, uploadInfo );
}

// ----------------------------------------------------------------------------------------------------

VertexBuffer::VertexBuffer( BufferInfo_t info )
{
	Globals::m_renderContext->CreateVertexBuffer( info, &m_handle );
}

IndexBuffer::IndexBuffer( BufferInfo_t info )
{
	Globals::m_renderContext->CreateIndexBuffer( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

RenderTexture::RenderTexture( RenderTextureInfo_t info )
{
	Globals::m_renderContext->CreateRenderTexture( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Descriptor::Descriptor( DescriptorInfo_t info )
{
	Globals::m_renderContext->CreateDescriptor( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Pipeline::Pipeline( PipelineInfo_t info )
{
	Globals::m_renderContext->CreatePipeline( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Shader::Shader( ShaderInfo_t info )
{
	Globals::m_renderContext->CreateShader( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------