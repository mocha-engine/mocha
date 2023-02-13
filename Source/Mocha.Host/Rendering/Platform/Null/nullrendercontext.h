#pragma once

#include <Misc/defs.h>
#include <Misc/globalvars.h>
#include <Misc/handlemap.h>
#include <Misc/mathtypes.h>
#include <Rendering/baserendercontext.h>
#include <shared_mutex>
#include <unordered_map>

// ----------------------------------------------------------------------------------------------------------------------------

class NullRenderContext : public BaseRenderContext
{
protected:
	// ----------------------------------------

	RenderStatus CreateImageTexture( ImageTextureInfo_t textureInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus CreateRenderTexture( RenderTextureInfo_t textureInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus SetImageTextureData( Handle handle, TextureData_t pipelineInfo ) override { return RENDER_STATUS_OK; }
	RenderStatus CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo ) override { return RENDER_STATUS_OK; }

	RenderStatus CreateBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus CreateVertexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus CreateIndexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus UploadBuffer( Handle handle, BufferUploadInfo_t pipelineInfo ) override { return RENDER_STATUS_OK; }

	RenderStatus CreatePipeline( PipelineInfo_t pipelineInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus CreateDescriptor( DescriptorInfo_t pipelineInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }
	RenderStatus CreateShader( ShaderInfo_t pipelineInfo, Handle* outHandle ) override { return RENDER_STATUS_OK; }

public:
	// ----------------------------------------

	NullRenderContext( Root* m_parent )
	    : BaseRenderContext( m_parent )
	{
	}

	/// <inheritdoc />
	RenderStatus Startup() override { return RENDER_STATUS_OK; }
	/// <inheritdoc />
	RenderStatus Shutdown() override { return RENDER_STATUS_OK; }
	/// <inheritdoc />
	RenderStatus BeginRendering() override { return RENDER_STATUS_OK; }
	/// <inheritdoc />
	RenderStatus EndRendering() override { return RENDER_STATUS_OK; }

	// ----------------------------------------

	/// <inheritdoc />
	RenderStatus BindPipeline( Pipeline p ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus BindDescriptor( Descriptor d ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus UpdateDescriptor( Descriptor d, DescriptorUpdateInfo_t updateInfo ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus BindVertexBuffer( VertexBuffer vb ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus BindIndexBuffer( IndexBuffer ib ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus BindConstants( RenderPushConstants p ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus BindRenderTarget( RenderTexture rt ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus GetRenderSize( Size2D* outSize ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus GetWindowSize( Size2D* outSize ) override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	void UpdateWindow() override {}

	/// <inheritdoc />
	bool GetWindowCloseRequested() override { return false; }

	/// <inheritdoc />
	RenderStatus GetGPUInfo( GPUInfo* outInfo ) override { return RENDER_STATUS_OK; }

	// ----------------------------------------

	/// <inheritdoc />
	RenderStatus BeginImGui() override { return RENDER_STATUS_OK; }
	/// <inheritdoc />
	RenderStatus EndImGui() override { return RENDER_STATUS_OK; }

	/// <inheritdoc />
	RenderStatus GetImGuiTextureID( ImageTexture* texture, void** outTextureId ) override { return RENDER_STATUS_OK; }
};
