#include "baserendercontext.h"

inline ImageTexture::ImageTexture( ImageTextureInfo_t info )
{
	g_renderContext->CreateImageTexture( info, &m_handle );
}

inline void ImageTexture::SetData( TextureData_t textureData )
{
	g_renderContext->SetImageTextureData( m_handle, textureData );
}

inline void ImageTexture::Copy( TextureCopyData_t copyData )
{
	g_renderContext->CopyImageTexture( m_handle, copyData );
}

// ----------------------------------------------------------------------------------------------------

inline BaseBuffer::BaseBuffer( BufferInfo_t info )
{
	g_renderContext->CreateBuffer( info, &m_handle );
}

inline void BaseBuffer::Upload( BufferUploadInfo_t uploadInfo )
{
	g_renderContext->UploadBuffer( m_handle, uploadInfo );
}

// ----------------------------------------------------------------------------------------------------

inline VertexBuffer::VertexBuffer( BufferInfo_t info )
{
	g_renderContext->CreateVertexBuffer( info, &m_handle );
}

inline IndexBuffer::IndexBuffer( BufferInfo_t info )
{
	g_renderContext->CreateIndexBuffer( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

inline RenderTexture::RenderTexture( RenderTextureInfo_t info )
{
	g_renderContext->CreateRenderTexture( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

inline Descriptor::Descriptor( DescriptorInfo_t info )
{
	g_renderContext->CreateDescriptor( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

inline Pipeline::Pipeline( PipelineInfo_t info )
{
	g_renderContext->CreatePipeline( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------

inline Shader::Shader( ShaderInfo_t info )
{
	g_renderContext->CreateShader( info, &m_handle );
}

// ----------------------------------------------------------------------------------------------------