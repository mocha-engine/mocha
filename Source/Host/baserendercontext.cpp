#include "baserendercontext.h"

float lastRenderScale = -1.0f;

// TODO: Cvar hooks so that we can change things when cvars change (i.e. re-create render targets when
// render.scale is changed)
FloatCVar renderScale( "render.scale", 1.0f, CVarFlags::Archive, "Multiplier for render resolution scaling" );

// ----------------------------------------------------------------------------------------------------

ImageTexture::ImageTexture( ImageTextureInfo_t info )
{
	g_renderContext->CreateImageTexture( info, &m_handle );
}

void ImageTexture::SetData( TextureData_t textureData )
{
	g_renderContext->SetImageTextureData( m_handle, textureData );
}

void ImageTexture::Copy( TextureCopyData_t copyData )
{
	g_renderContext->CopyImageTexture( m_handle, copyData );
}

// ----------------------------------------------------------------------------------------------------

BaseBuffer::BaseBuffer( BufferInfo_t info )
{
	g_renderContext->CreateBuffer( info, &m_handle );
}

void BaseBuffer::Upload( BufferUploadInfo_t uploadInfo )
{
	g_renderContext->UploadBuffer( m_handle, uploadInfo );
}

// ----------------------------------------------------------------------------------------------------

VertexBuffer::VertexBuffer( BufferInfo_t info )
{
	g_renderContext->CreateVertexBuffer( info, &m_handle );
}

IndexBuffer::IndexBuffer( BufferInfo_t info )
{
	g_renderContext->CreateIndexBuffer( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

RenderTexture::RenderTexture( RenderTextureInfo_t info )
{
	g_renderContext->CreateRenderTexture( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Descriptor::Descriptor( DescriptorInfo_t info )
{
	g_renderContext->CreateDescriptor( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Pipeline::Pipeline( PipelineInfo_t info )
{
	g_renderContext->CreatePipeline( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Shader::Shader( ShaderInfo_t info )
{
	g_renderContext->CreateShader( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------