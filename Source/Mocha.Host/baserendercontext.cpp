#include "baserendercontext.h"

#include <clientroot.h>

float lastRenderScale = -1.0f;

// TODO: Cvar hooks so that we can change things when cvars change (i.e. re-create render targets when
// render.scale is changed)
FloatCVar renderScale( "render.scale", 1.0f, CVarFlags::Archive, "Multiplier for render resolution scaling" );

// ----------------------------------------------------------------------------------------------------

ImageTexture::ImageTexture( Root* parent, ImageTextureInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreateImageTexture( info, &m_handle );
}

void ImageTexture::SetData( TextureData_t textureData )
{
	m_parent->m_renderContext->SetImageTextureData( m_handle, textureData );
}

void ImageTexture::Copy( TextureCopyData_t copyData )
{
	m_parent->m_renderContext->CopyImageTexture( m_handle, copyData );
}

// ----------------------------------------------------------------------------------------------------

BaseBuffer::BaseBuffer( Root* parent, BufferInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreateBuffer( info, &m_handle );
}

void BaseBuffer::Upload( BufferUploadInfo_t uploadInfo )
{
	m_parent->m_renderContext->UploadBuffer( m_handle, uploadInfo );
}

// ----------------------------------------------------------------------------------------------------

VertexBuffer::VertexBuffer( Root* parent, BufferInfo_t info )
{
	m_parent = parent;
	m_parent->m_renderContext->CreateVertexBuffer( info, &m_handle );
}

IndexBuffer::IndexBuffer( Root* parent, BufferInfo_t info )
{
	m_parent = parent;
	m_parent->m_renderContext->CreateIndexBuffer( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

RenderTexture::RenderTexture( Root* parent, RenderTextureInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreateRenderTexture( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Descriptor::Descriptor( Root* parent, DescriptorInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreateDescriptor( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Pipeline::Pipeline( Root* parent, PipelineInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreatePipeline( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

Shader::Shader( Root* parent, ShaderInfo_t info )
    : RenderObject( parent )
{
	m_parent->m_renderContext->CreateShader( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------