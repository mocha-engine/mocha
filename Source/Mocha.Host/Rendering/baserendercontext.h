#pragma once
#include <Entities/baseentity.h>
#include <Misc/cvarmanager.h>
#include <Misc/defs.h>
#include <Util/util.h>
#include <cstdint>
#include <string>

// ----------------------------------------------------------------------------------------------------
extern FloatCVar renderScale;
extern float lastRenderScale;

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
class Descriptor;
class Root;

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

enum BufferUsageFlags
{
	BUFFER_USAGE_FLAG_VERTEX_BUFFER = 1 << 1,
	BUFFER_USAGE_FLAG_INDEX_BUFFER = 1 << 2,
	BUFFER_USAGE_FLAG_UNIFORM_BUFFER = 1 << 3,
	BUFFER_USAGE_FLAG_TRANSFER_SRC = 1 << 4,
	BUFFER_USAGE_FLAG_TRANSFER_DST = 1 << 5
};

enum ShaderType
{
	SHADER_TYPE_VERTEX,
	SHADER_TYPE_FRAGMENT
};

DEFINE_FLAG_OPERATORS( BufferUsageFlags );

// ----------------------------------------------------------------------------------------------------

struct TextureInfo_t
{
	uint32_t width = 0;
	uint32_t height = 0;

	// This should be 1 or greater.
	uint32_t mipCount = 1;
};

struct RenderTextureInfo_t : public TextureInfo_t
{
	std::string name = "Unnamed Render Texture";
	RenderTextureType type;
};

struct ImageTextureInfo_t : public TextureInfo_t
{
	std::string name = "Unnamed Image Texture";
};

struct TextureData_t
{
	uint32_t width = 0;
	uint32_t height = 0;
	uint32_t mipCount = 1;

	// This UtilArray can contain any texture data as long
	// as it matches the image format specified.
	UtilArray mipData = {};
	int imageFormat = 0;
};

struct TextureCopyData_t
{
	uint32_t srcX = 0;
	uint32_t srcY = 0;
	uint32_t dstX = 0;
	uint32_t dstY = 0;
	uint32_t width = 0;
	uint32_t height = 0;
	ImageTexture* src = nullptr;
};

struct BufferInfo_t
{
	std::string name = "Unnamed Buffer";
	uint32_t size = 0;
	BufferType type = BUFFER_TYPE_STAGING;
	BufferUsageFlags usage = BUFFER_USAGE_FLAG_INDEX_BUFFER;
};

struct BufferUploadInfo_t
{
	UtilArray data = {};
};

struct DescriptorBindingInfo_t
{
	DescriptorBindingType type = DESCRIPTOR_BINDING_TYPE_IMAGE;
	ImageTexture* texture = nullptr;

	bool isBindless = false;
};

struct DescriptorInfo_t
{
	std::string name = "Unnamed Descriptor";
	std::vector<DescriptorBindingInfo_t> bindings = {};
};

struct DescriptorUpdateInfo_t
{
	int binding = 0;
	ImageTexture* src = nullptr;
	SamplerType samplerType = SAMPLER_TYPE_ANISOTROPIC;
};

struct ShaderInfo_t
{
	std::string name = "Unnamed Shader";
	std::vector<uint32_t> fragmentShaderData;
	std::vector<uint32_t> vertexShaderData;
};

struct VertexAttributeInfo_t
{
	std::string name = "";
	VertexAttributeFormat format = VERTEX_ATTRIBUTE_FORMAT_FLOAT;
};

struct PipelineInfo_t
{
	std::string name = "Unnamed Pipeline";
	ShaderInfo_t shaderInfo = {};
	std::vector<Descriptor*> descriptors = {};
	std::vector<VertexAttributeInfo_t> vertexAttributes = {};
	bool ignoreDepth = false;
	bool renderToSwapchain = false;
};

struct RenderPushConstants
{
	glm::vec4 data = glm::vec4{ 1.0f };

	glm::mat4 modelMatrix = glm::mat4{ 1.0f };

	glm::mat4 renderMatrix = glm::mat4{ 1.0f };

	glm::vec3 cameraPos = glm::vec3{ 1.0f };
	float time = 0.0f;

	glm::vec4 vLightInfoWS[4] = {};
};

struct GPUInfo
{
	const char* gpuName = "Unnamed";
};

// ----------------------------------------------------------------------------------------------------

class RenderObject
{
public:
	Handle m_handle = HANDLE_INVALID;

	inline bool IsValid() { return m_handle != HANDLE_INVALID; }
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
	RENDER_STATUS_SHADER_COMPILE_FAILED,				// The shader failed to compile
	RENDER_STATUS_WINDOW_SIZE_INVALID,					// The window size is invalid. It might be minimized. This shouldn't be treated as an error.
};

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
		"RENDER_STATUS_INVALID_HANDLE",
		"RENDER_STATUS_SHADER_COMPILE_FAILED",
		"RENDER_STATUS_WINDOW_SIZE_INVALID"
	};

	return RenderContextStatusStrings[status];
}

// clang-format on
// ----------------------------------------------------------------------------------------------------

inline void ErrorIf( bool condition, RenderStatus status )
{
	if ( condition )
	{
		std::string error = "RenderContext Error: " + GetRenderContextStatusString( status );
		ErrorMessage( error );
		abort();
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

	virtual RenderStatus CreateImageTexture( ImageTextureInfo_t textureInfo, Handle* outHandle ) = 0;
	virtual RenderStatus CreateRenderTexture( RenderTextureInfo_t textureInfo, Handle* outHandle ) = 0;
	virtual RenderStatus SetImageTextureData( Handle handle, TextureData_t pipelineInfo ) = 0;
	virtual RenderStatus CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo ) = 0;

	virtual RenderStatus CreateBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	virtual RenderStatus CreateVertexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	virtual RenderStatus CreateIndexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) = 0;
	virtual RenderStatus UploadBuffer( Handle handle, BufferUploadInfo_t pipelineInfo ) = 0;

	virtual RenderStatus CreatePipeline( PipelineInfo_t pipelineInfo, Handle* outHandle ) = 0;
	virtual RenderStatus CreateDescriptor( DescriptorInfo_t pipelineInfo, Handle* outHandle ) = 0;
	virtual RenderStatus CreateShader( ShaderInfo_t pipelineInfo, Handle* outHandle ) = 0;

public:
	// All render types should be able to access render context internals
	// for object creation etc.
	friend ImageTexture;
	friend RenderTexture;
	friend BaseBuffer;
	friend VertexBuffer;
	friend IndexBuffer;
	friend Pipeline;
	friend Descriptor;
	friend Shader;

	// ----------------------------------------
	// Startup / shutdown
	// ----------------------------------------

	virtual RenderStatus Startup() = 0;
	virtual RenderStatus Shutdown() = 0;

	// ----------------------------------------
	// Rendering commands
	// ----------------------------------------

	/// <summary>
	/// Begins rendering. Call this before invoking any render functions.
	/// This should be matched with a call to EndRendering.
	/// </summary>
	/// <returns>
	/// <para>
	/// <b>RENDER_STATUS_OK</b> if successful
	/// </para>
	/// <para>
	/// <b>RENDER_STATUS_WINDOW_SIZE_INVALID</b> if the window has been minimized or made too small.
	/// Do not call EndRendering if this is returned.
	/// </para>
	/// </returns>
	virtual RenderStatus BeginRendering() = 0;

	/// <summary>
	/// Ends rendering.
	/// This should be matched with a call to BeginRendering.
	/// </summary>
	/// <returns>
	/// <para>
	/// <b>RENDER_STATUS_OK</b> if successful
	/// </para>
	/// <para>
	/// <b>RENDER_STATUS_WINDOW_SIZE_INVALID</b> if the window has been minimized or made too small.
	/// </para>
	/// </returns>
	virtual RenderStatus EndRendering() = 0;

	// ----------------------------------------
	// Low-level rendering
	// ----------------------------------------

	/// <summary>
	/// Binds a pipeline
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindPipeline( Pipeline p ) = 0;

	/// <summary>
	/// Binds a descriptor
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindDescriptor( Descriptor d ) = 0;

	/// <summary>
	/// Updates a descriptor
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus UpdateDescriptor( Descriptor d, DescriptorUpdateInfo_t updateInfo ) = 0;

	/// <summary>
	/// Binds a vertex buffer
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindVertexBuffer( VertexBuffer vb ) = 0;

	/// <summary>
	/// Binds an index buffer
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindIndexBuffer( IndexBuffer ib ) = 0;

	/// <summary>
	/// Binds rendering push constants
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindConstants( RenderPushConstants p ) = 0;

	/// <summary>
	/// Draws the contents of the vertex and/or index buffer
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount ) = 0;

	/// <summary>
	/// Call this to set the render target to render to.
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BindRenderTarget( RenderTexture rt ) = 0;

	/// <summary>
	/// This will return the size for the current render target.
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus GetRenderSize( Size2D* outSize ) = 0;

	/// <summary>
	/// Get information about the GPU.
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus GetGPUInfo( GPUInfo* outInfo ) = 0;

	// ----------------------------------------
	// High-level rendering
	// ----------------------------------------

	/// <summary>
	/// Begin the ImGUI drawing pass.
	/// This will handle all ImGui::NewFrame functions, e.g.
	/// "ImGui_ImplVulkan_NewFrame()".
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus BeginImGui() = 0;

	/// <summary>
	/// End the ImGUI drawing pass.
	/// This will call ImGui::Render and handle multi-viewports.
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus EndImGui() = 0;

	/// <summary>
	/// This will return a pointer that ImGui can use to draw a texture.
	/// </summary>
	/// <returns><b>RENDER_STATUS_OK</b> if successful, otherwise an error code</returns>
	virtual RenderStatus GetImGuiTextureID( ImageTexture* texture, void** outTextureId ) = 0;

	// ----------------------------------------
	// Windowing
	// ----------------------------------------

	// TODO: Move these elsewhere

	/// <summary>
	/// Get the current window size
	/// </summary>
	virtual RenderStatus GetWindowSize( Size2D* outSize ) = 0;

	/// <summary>
	/// Update window, fetch inputs etc..
	/// </summary>
	virtual void UpdateWindow() = 0;

	/// <summary>
	/// Did the user try to close the game window?
	/// </summary>
	virtual bool GetWindowCloseRequested() = 0;

	// ----------------------------------------
	// ImGui
	// ----------------------------------------

	ImFont* m_mainFont;
	ImFont* m_monospaceFont;
	ImFont* m_headingFont;
	ImFont* m_boldFont;
	ImFont* m_subheadingFont;
};