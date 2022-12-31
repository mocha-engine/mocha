#pragma once
#include <baseentity.h>
#include <cstdint>
#include <defs.h>
#include <string>

// ----------------------------------------------------------------------------------------------------
// clang-format off

enum RenderObjectStatus
{
	STATUS_OK,							// No error
	STATUS_ERROR,						// Generic error
};

#define RENDEROBJECT_FUNC virtual RenderObjectStatus

// clang-format on
// ----------------------------------------------------------------------------------------------------

struct TextureData_t
{
	uint32_t width;
	uint32_t height;
	uint32_t mipCount;
	InteropArray mipData;
	int imageFormat;
};

struct TextureCopyData_t
{
	uint32_t srcX;
	uint32_t srcY;
	uint32_t dstX;
	uint32_t dstY;
	uint32_t width;
	uint32_t height;
	Texture* src;
};

// ----------------------------------------------------------------------------------------------------

enum RenderTextureType
{
	RENDER_TEXTURE_COLOR,
	RENDER_TEXTURE_COLOR_OPAQUE,
	RENDER_TEXTURE_DEPTH
};

// ----------------------------------------------------------------------------------------------------

enum SamplerType
{
	SAMPLER_TYPE_POINT,
	SAMPLER_TYPE_LINEAR,
	SAMPLER_TYPE_ANISOTROPIC
};

// ----------------------------------------------------------------------------------------------------
class BaseTexture
{
public:
};

class ImageTexture : public BaseTexture
{
public:
	RENDEROBJECT_FUNC SetData( TextureData_t textureData ) = 0;
	RENDEROBJECT_FUNC Copy( TextureCopyData_t copyData ) = 0;
};

class RenderTexture : public BaseTexture
{
public:
	
};

// ----------------------------------------

class VertexBuffer
{
public:
};

class IndexBuffer
{
public:
};

// ----------------------------------------

class Pipeline
{
public:
};

class Descriptor
{
public:
};

// ----------------------------------------------------------------------------------------------------
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

#define RENDERCONTEXT_FUNC virtual RenderContextStatus

// clang-format on
// ----------------------------------------------------------------------------------------------------

inline std::string GetRenderContextStatusString( RenderContextStatus status )
{
	const std::string RenderContextStatusStrings[] = {
	    "STATUS_OK",
	    "NOT_INITIALIZED",
	    "ALREADY_INITIALIZED",
	    "BEGIN_END_MISMATCH",
	    "NO_PIPELINE_BOUND",
	    "NO_VERTEX_BUFFER_BOUND",
	    "NO_INDEX_BUFFER_BOUND",
	};

	return RenderContextStatusStrings[status];
}

// ----------------------------------------------------------------------------------------------------

inline void ErrorIf( bool condition, RenderContextStatus status )
{
	if ( condition )
	{
		std::string error = "RenderContext Error: " + GetRenderContextStatusString( status );
		ERRORMESSAGE( error );
	}
}

// ----------------------------------------------------------------------------------------------------

// This is the base class for all rendering contexts.
class BaseRenderContext
{
protected:
	// ----------------------------------------
	// Internal state
	// ----------------------------------------

	// Was Startup called?
	bool m_hasInitialized;

	// Was BeginRendering called?
	bool m_renderingActive;

	// The current pipeline
	Pipeline* m_currentPipeline;

	// The current vertex buffer
	VertexBuffer* m_currentVertexBuffer;

public:
	// ----------------------------------------
	// Startup / shutdown
	// ----------------------------------------

	RENDERCONTEXT_FUNC Startup() = 0;
	RENDERCONTEXT_FUNC Shutdown() = 0;

	// ----------------------------------------
	// Rendering commands
	// ----------------------------------------

	// Call this before invoking any render functions.
	RENDERCONTEXT_FUNC BeginRendering() = 0;

	// Call this after you're done.
	RENDERCONTEXT_FUNC EndRendering() = 0;

	// ----------------------------------------
	//
	// Low-level rendering
	//
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

	// ----------------------------------------
	//
	// High-level rendering
	//
	// Render an entity. This will handle all the pipelines, descriptors, buffers, etc. for you - just call
	// this once and it'll do all the work.
	// Note that this will render to whatever render target is currently bound (see BindRenderTarget).
	RENDERCONTEXT_FUNC RenderEntity( BaseEntity* entity ) = 0;
};

#undef RENDERCONTEXT_FUNC