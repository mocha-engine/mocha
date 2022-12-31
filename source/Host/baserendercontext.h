#pragma once
#include <cstdint>

class BaseTexture;
class ImageTexture;
class RenderTexture;

class VertexBuffer;
class IndexBuffer;

class Pipeline;
class Descriptor;

// clang-format off
enum RenderContextStatus
{
	STATUS_OK,							// No error
	NOT_INITIALIZED,					// You didn't call Startup()
	ALREADY_INITIALIZED,				// You already called Startup()
	BEGIN_END_MISMATCH,					// You called EndRendering or forgot to call BeginRendering, and then invoked a render function
	NO_PIPELINE_BOUND,					// You tried to render without a pipeline bound
	NO_VERTEX_BUFFER_BOUND,				// You tried to render without a vertex buffer bound
	NO_INDEX_BUFFER_BOUND,				// You tried to render without an index buffer bound and had indexCount > 0
};
// clang-format on

#define RETURN_ERROR_IF( x, e ) \
	if ( x )                    \
	{                           \
		return e;               \
	}

#define RENDERCONTEXT_FUNC virtual RenderContextStatus

// This is the base class for all rendering contexts.
class BaseRenderContext
{
protected:
	//
	// Internal state
	//

	// Was Startup called?
	bool m_hasInitialized;

	// Was BeginRendering called?
	bool m_renderingActive;

	// The current pipeline
	Pipeline* m_currentPipeline;

	// The current vertex buffer
	VertexBuffer* m_currentVertexBuffer;

public:
	//
	// Startup / shutdown
	//
	RENDERCONTEXT_FUNC Startup() = 0;
	RENDERCONTEXT_FUNC Shutdown() = 0;

	//
	// Rendering commands
	//

	// Call this before invoking any render functions.
	RENDERCONTEXT_FUNC BeginRendering() = 0;

	// Binds a pipeline
	RENDERCONTEXT_FUNC BindPipeline( Pipeline p ) = 0;

	// Binds a descriptor
	RENDERCONTEXT_FUNC BindDescriptor( Descriptor d ) = 0;

	// Binds a vertex buffer
	RENDERCONTEXT_FUNC BindVertexBuffer( VertexBuffer vb ) = 0;

	// Binds an index buffer
	RENDERCONTEXT_FUNC BindIndexBuffer( IndexBuffer ib ) = 0;

	// Draws the contents of the vertex and/or index buffer
	RENDERCONTEXT_FUNC Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount ) = 0;

	// Call this to set the render target to render to.
	RENDERCONTEXT_FUNC BindRenderTarget( RenderTexture rt ) = 0;

	// Call this after you're done.
	RENDERCONTEXT_FUNC EndRendering() = 0;
};