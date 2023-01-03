#pragma once
#include <baseentity.h>
#include <cstdint>
#include <defs.h>
#include <string>
#include <util.h>

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

struct Mesh;
class BaseRenderContext;
class ImageTexture;

// ----------------------------------------------------------------------------------------------------

enum RenderTextureType
{
	RENDER_TEXTURE_COLOR,
	RENDER_TEXTURE_COLOR_OPAQUE,
	RENDER_TEXTURE_DEPTH
};

enum SamplerType
{
	SAMPLER_TYPE_POINT,
	SAMPLER_TYPE_LINEAR,
	SAMPLER_TYPE_ANISOTROPIC
};

enum BufferType
{
	BUFFER_TYPE_STAGING,
	BUFFER_TYPE_VERTEX_INDEX_DATA,
	BUFFER_TYPE_UNIFORM_DATA
};

enum DescriptorBindingType
{
	DESCRIPTOR_BINDING_TYPE_IMAGE
};

enum VertexAttributeFormat
{
	VERTEX_ATTRIBUTE_FORMAT_INT,
	VERTEX_ATTRIBUTE_FORMAT_FLOAT,
	VERTEX_ATTRIBUTE_FORMAT_FLOAT2,
	VERTEX_ATTRIBUTE_FORMAT_FLOAT3,
	VERTEX_ATTRIBUTE_FORMAT_FLOAT4
};

// ----------------------------------------------------------------------------------------------------

struct TextureInfo_t
{
	uint32_t width;
	uint32_t height;
	uint32_t mipCount;
};

struct RenderTextureInfo_t : public TextureInfo_t
{
	RenderTextureType type;
};

struct ImageTextureInfo_t : public TextureInfo_t
{
	// TODO: Replace
	int imageFormat;
};

struct TextureData_t
{
	uint32_t width;
	uint32_t height;
	uint32_t mipCount;
	UtilArray mipData;
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
	ImageTexture* src;
};

struct BufferInfo_t
{
	uint32_t size;
	BufferType type;
};

struct BufferUploadInfo_t
{
	UtilArray data;
};

struct DescriptorBindingInfo_t
{
	DescriptorBindingType type;
	SamplerType samplerType;
	ImageTexture* texture;
};

struct DescriptorInfo_t
{
	std::vector<DescriptorBindingInfo_t> bindings;
};

struct ShaderInfo_t
{
	std::string shaderPath;
};

struct VertexAttributeInfo_t
{
	std::string name;
	VertexAttributeFormat format;
};

struct PipelineInfo_t
{
	ShaderInfo_t shaderInfo;
	std::vector<DescriptorInfo_t> descriptors;
	std::vector<VertexAttributeInfo_t> vertexAttributes;
	bool ignoreDepth;
};

// ----------------------------------------------------------------------------------------------------

class RenderObject
{
public:
	Handle m_handle;
};

class ImageTexture : public RenderObject
{
public:
	ImageTexture() {}
	ImageTexture( ImageTextureInfo_t info );

	void SetData( TextureData_t textureData );
	void Copy( TextureCopyData_t copyData );
};

class RenderTexture : public RenderObject
{
public:
	RenderTexture() {}
	RenderTexture( RenderTextureInfo_t info );
};

// ----------------------------------------

class BaseBuffer : public RenderObject
{
public:
	BaseBuffer() {}

	BaseBuffer( BufferInfo_t info );
	void Upload( BufferUploadInfo_t uploadInfo );
};

class VertexBuffer : public BaseBuffer
{
public:
	VertexBuffer() {}
	VertexBuffer( BufferInfo_t info );
};

class IndexBuffer : public BaseBuffer
{
public:
	IndexBuffer() {}
	IndexBuffer( BufferInfo_t info );
};

// ----------------------------------------

class Pipeline : public RenderObject
{
public:
	Pipeline() {}
	Pipeline( PipelineInfo_t info );
};

class Descriptor : public RenderObject
{
public:
	Descriptor() {}
	Descriptor( DescriptorInfo_t info );
};

class Shader : public RenderObject
{
public:
	Shader() {}
	Shader( ShaderInfo_t info );
};

// ----------------------------------------------------------------------------------------------------
// clang-format off

enum RenderStatus
{
	RENDER_STATUS_OK,									// No error
	RENDER_STATUS_NOT_INITIALIZED,						// You didn't call Startup()
	RENDER_STATUS_ALREADY_INITIALIZED,					// You already called Startup()
	RENDER_STATUS_BEGIN_END_MISMATCH,					// You called EndRendering or forgot to call BeginRendering, and then invoked a render function
	RENDER_STATUS_NO_PIPELINE_BOUND,					// You tried to render without a pipeline bound
	RENDER_STATUS_NO_VERTEX_BUFFER_BOUND,				// You tried to render without a vertex buffer bound
	RENDER_STATUS_NO_INDEX_BUFFER_BOUND,				// You tried to render without an index buffer bound and had indexCount > 0
	RENDER_STATUS_INVALID_HANDLE,						// You passed an invalid handle to a render function
};

#define RENDERCONTEXT_FUNC virtual RenderStatus

// clang-format on
// ----------------------------------------------------------------------------------------------------

inline std::string GetRenderContextStatusString( RenderStatus status )
{
	const std::string RenderContextStatusStrings[] = {
	    "RENDER_STATUS_OK",
	    "RENDER_STATUS_NOT_INITIALIZED",
	    "RENDER_STATUS_ALREADY_INITIALIZED",
	    "RENDER_STATUS_BEGIN_END_MISMATCH",
	    "RENDER_STATUS_NO_PIPELINE_BOUND",
	    "RENDER_STATUS_NO_VERTEX_BUFFER_BOUND",
	    "RENDER_STATUS_NO_INDEX_BUFFER_BOUND",
	};

	return RenderContextStatusStrings[status];
}

// ----------------------------------------------------------------------------------------------------

inline void ErrorIf( bool condition, RenderStatus status )
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

	// ----------------------------------------
	// Objects
	// ----------------------------------------
	RENDERCONTEXT_FUNC CreateImageTexture( ImageTextureInfo_t textureInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC CreateRenderTexture( RenderTextureInfo_t textureInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC SetImageTextureData( Handle handle, TextureData_t pipelineInfo ) = 0;
	RENDERCONTEXT_FUNC CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo ) = 0;

	RENDERCONTEXT_FUNC CreateBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC CreateVertexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC CreateIndexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC UploadBuffer( Handle handle, BufferUploadInfo_t pipelineInfo ) = 0;

	RENDERCONTEXT_FUNC CreatePipeline( PipelineInfo_t pipelineInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC CreateDescriptor( DescriptorInfo_t pipelineInfo, Handle* outHandle ) = 0;
	RENDERCONTEXT_FUNC CreateShader( ShaderInfo_t pipelineInfo, Handle* outHandle ) = 0;

public:
#define FRIEND( x ) friend class x
	// All render types should be able to access render context internals
	// for object creation etc.
	FRIEND( ImageTexture );
	FRIEND( RenderTexture );
	FRIEND( BaseBuffer );
	FRIEND( VertexBuffer );
	FRIEND( IndexBuffer );
	FRIEND( Pipeline );
	FRIEND( Descriptor );
	FRIEND( Shader );
#undef FRIEND

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

	// This will return the size for the current render target.
	RENDERCONTEXT_FUNC GetRenderSize( Size2D* outSize ) = 0;

	// ----------------------------------------
	//
	// High-level rendering
	//
	// Render a mesh. This will handle all the pipelines, descriptors, buffers, etc. for you - just call
	// this once and it'll do all the work.
	// Note that this will render to whatever render target is currently bound (see BindRenderTarget).
	RENDERCONTEXT_FUNC RenderMesh( Mesh* mesh ) = 0;
};

#undef RENDERCONTEXT_FUNC