#pragma once

#include <VkBootstrap.h>
#include <baserendercontext.h>
#include <defs.h>
#include <globalvars.h>
#include <handlemap.h>
#include <mathtypes.h>
#include <vkinit.h>
#include <vulkan/vulkan.h>
#include <window.h>
#include <vk_mem_alloc.h>

// ----------------------------------------------------------------------------------------------------------------------------

// Forward decls
class VulkanRenderContext;

// ----------------------------------------------------------------------------------------------------------------------------

struct VulkanVertexInputDescription
{
	std::vector<VkVertexInputBindingDescription> bindings;
	std::vector<VkVertexInputAttributeDescription> attributes;

	VkPipelineVertexInputStateCreateFlags flags = 0;
};

// ----------------------------------------------------------------------------------------------------------------------------

struct VulkanObject
{
protected:
	VulkanRenderContext* m_parent; // This MUST never be null. If an object exists, then it should have
	                               // a parent context.

	void SetParent( VulkanRenderContext* parent )
	{
		assert( parent != nullptr && "Parent was nullptr" );
		m_parent = parent;
	}
};

// ----------------------------------------------------------------------------------------------------------------------------

struct VulkanBuffer : public VulkanObject
{
private:
	VkBufferUsageFlags GetBufferUsageFlags( BufferUsageFlags flags );

public:
	VkBuffer buffer;
	VmaAllocation allocation;

	VulkanBuffer() {}
	VulkanBuffer( VulkanRenderContext* parent, BufferInfo_t bufferInfo, VmaMemoryUsage memoryUsage );

	void SetData( BufferUploadInfo_t uploadInfo );
};

// ----------------------------------------------------------------------------------------------------------------------------

struct VulkanSampler : public VulkanObject
{
private:
	VkSamplerCreateInfo GetCreateInfo( SamplerType samplerType );

public:
	VkSampler sampler;

	VulkanSampler() {}
	VulkanSampler( VulkanRenderContext* parent, SamplerType samplerType );
};

// ----------------------------------------------------------------------------------------------------------------------------

struct VulkanCommandContext : public VulkanObject
{
	VkCommandPool commandPool;
	VkCommandBuffer commandBuffer;
	VkFence fence;

	VulkanCommandContext() {}
	VulkanCommandContext( VulkanRenderContext* parent );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanRenderTexture : public VulkanObject
{
private:
	VkImageUsageFlagBits GetUsageFlagBits( RenderTextureType type );
	VkFormat GetFormat( RenderTextureType type );
	VkImageAspectFlags GetAspectFlags( RenderTextureType type );

public:
	VkImage image;
	VmaAllocation allocation;
	VkImageView imageView;
	VkFormat format;

	VulkanRenderTexture() {}
	VulkanRenderTexture( VulkanRenderContext* parent ) { SetParent( parent ); }
	VulkanRenderTexture( VulkanRenderContext* parent, RenderTextureInfo_t textureInfo );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanImageTexture : public VulkanObject
{
private:
	inline int GetTexelBlockSize( VkFormat format )
	{
		switch ( format )
		{
		case VkFormat::VK_FORMAT_R8G8B8A8_SRGB:
		case VkFormat::VK_FORMAT_R8G8B8A8_UNORM:
			return 4;
			break;
		case VkFormat::VK_FORMAT_BC3_SRGB_BLOCK:
		case VkFormat::VK_FORMAT_BC3_UNORM_BLOCK:
			return 1;
			break;
		case VkFormat::VK_FORMAT_BC5_UNORM_BLOCK:
		case VkFormat::VK_FORMAT_BC5_SNORM_BLOCK:
			return 1;
			break;
		}

		assert( false && "Format is not supported." ); // Format is not supported
		return -1;
	}

	inline void GetMipDimensions(
	    uint32_t inWidth, uint32_t inHeight, uint32_t mipLevel, uint32_t* outWidth, uint32_t* outHeight )
	{
		*outWidth = inWidth >> mipLevel;
		*outHeight = inHeight >> mipLevel;
	}

	inline int CalcMipSize( uint32_t inWidth, uint32_t inHeight, uint32_t mipLevel, VkFormat format )
	{
		uint32_t outWidth, outHeight;
		GetMipDimensions( inWidth, inHeight, mipLevel, &outWidth, &outHeight );
		return outWidth * outHeight * GetTexelBlockSize( format );
	}

public:
	VkImage image;
	VmaAllocation allocation;
	VkImageView imageView;
	VkFormat format;

	VulkanImageTexture() {}
	VulkanImageTexture( VulkanRenderContext* parent, ImageTextureInfo_t textureInfo );

	void SetData( TextureData_t textureData );
	void Copy( TextureCopyData_t copyData );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanSwapchain : public VulkanObject
{
private:
	void CreateMainSwapchain( Size2D size );
	void CreateDepthTexture( Size2D size );

public:
	VkSwapchainKHR m_swapchain = 0;
	VulkanRenderTexture m_depthTexture;
	std::vector<VulkanRenderTexture> m_swapchainTextures;

	uint32_t AcquireSwapchainImageIndex( VkDevice device, VkSemaphore presentSemaphore, VulkanCommandContext mainContext )
	{
		uint32_t swapchainImageIndex;

		VK_CHECK( vkAcquireNextImageKHR( device, m_swapchain, 1000000000, presentSemaphore, nullptr, &swapchainImageIndex ) );
		VK_CHECK( vkResetCommandBuffer( mainContext.commandBuffer, 0 ) );

		return swapchainImageIndex;
	}

	VulkanSwapchain() {}
	VulkanSwapchain( VulkanRenderContext* parent, Size2D size );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanDescriptor : public VulkanObject
{
private:
	VkDescriptorType GetDescriptorType( DescriptorBindingType type );

public:
	VkDescriptorSet descriptorSet;
	VkDescriptorSetLayout descriptorSetLayout;

	SamplerType samplerType;

	VulkanDescriptor() {}
	VulkanDescriptor( VulkanRenderContext* parent, DescriptorInfo_t descriptorInfo );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanShader : public VulkanObject
{
private:
	void LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

public:
	VkShaderModule vertexShader;
	VkShaderModule fragmentShader;

	VulkanShader() {}
	VulkanShader( VulkanRenderContext* parent, ShaderInfo_t shaderInfo );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanPipeline : public VulkanObject
{
private:
	VkFormat GetVulkanFormat( VertexAttributeFormat format );
	uint32_t GetSizeOf( VertexAttributeFormat format );

public:
	VkPipeline pipeline;
	VkPipelineLayout layout;

	VulkanPipeline(){}
	VulkanPipeline( VulkanRenderContext* parent, PipelineInfo_t pipelineInfo );
};

// ----------------------------------------------------------------------------------------------------------------------------

class VulkanRenderContext : public BaseRenderContext
{
private:
	//
	// Vulkan-specifics
	//
	VkDebugUtilsMessengerEXT m_debugMessenger;
	VkPhysicalDevice m_chosenGPU;
	VkDevice m_device;
	VkInstance m_instance;
	VkPhysicalDeviceProperties m_deviceProperties;
	VkQueue m_graphicsQueue;
	uint32_t m_graphicsQueueFamily;
	VkSurfaceKHR m_surface;
	VkSemaphore m_presentSemaphore, m_renderSemaphore;
	VkDescriptorPool m_descriptorPool;

	std::unique_ptr<Window> m_window;
	VulkanCommandContext m_mainContext;
	VulkanCommandContext m_uploadContext;
	VulkanSwapchain m_swapchain;
	VulkanSampler m_anisoSampler, m_pointSampler;

	// Create a Vulkan context, set up devices
	vkb::Instance CreateInstanceAndSurface();
	void FinalizeAndCreateDevice( vkb::PhysicalDevice physicalDevice );
	vkb::PhysicalDevice CreatePhysicalDevice( vkb::Instance vkbInstance );

	void CreateSwapchain();
	void CreateCommands();
	void CreateSyncStructures();
	void CreateDescriptors();
	void CreateSamplers();

	//
	// Vulkan memory allocator
	//
	VmaAllocator m_allocator;
	void CreateAllocator();

	//
	// State
	//
	uint32_t m_swapchainImageIndex;
	// Current swapchain target image. Refers to m_swapchain.images[currentImageIndex]
	VulkanRenderTexture m_swapchainTarget;
	// Current swapchain target depth buffer.
	VulkanRenderTexture m_depthTarget;

	// Current pipeline. Used when binding descriptors
	std::shared_ptr<VulkanPipeline> m_pipeline;

	// Checks to see whether the current window size is valid for rendering.
	inline bool CanRender();

	//
	// Handle maps
	// We refer to these in our external 'base' objects..
	// struct Texture
	// {
	//    public Handle handle;
	//
	//    Texture( /* ... */ )
	//    {
	//         handle = RenderContext::CreateImageTexture( /* ... */ );
	//    }
	// }
	//
	HandleMap<VulkanBuffer> m_buffers = {};
	HandleMap<VulkanImageTexture> m_imageTextures = {};
	HandleMap<VulkanRenderTexture> m_renderTextures = {};
	HandleMap<VulkanDescriptor> m_descriptors = {};
	HandleMap<VulkanPipeline> m_pipelines = {};
	HandleMap<VulkanShader> m_shaders = {};

	//
	// Immediate submit
	//
	RenderStatus ImmediateSubmit( std::function<RenderStatus( VkCommandBuffer commandBuffer )> func );

protected:
	// ----------------------------------------

	RenderStatus CreateImageTexture( ImageTextureInfo_t textureInfo, Handle* outHandle ) override;
	RenderStatus CreateRenderTexture( RenderTextureInfo_t textureInfo, Handle* outHandle ) override;
	RenderStatus SetImageTextureData( Handle handle, TextureData_t pipelineInfo ) override;
	RenderStatus CopyImageTexture( Handle handle, TextureCopyData_t pipelineInfo ) override;

	RenderStatus CreateBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override;
	RenderStatus CreateVertexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override;
	RenderStatus CreateIndexBuffer( BufferInfo_t bufferInfo, Handle* outHandle ) override;
	RenderStatus UploadBuffer( Handle handle, BufferUploadInfo_t pipelineInfo ) override;

	RenderStatus CreatePipeline( PipelineInfo_t pipelineInfo, Handle* outHandle ) override;
	RenderStatus CreateDescriptor( DescriptorInfo_t pipelineInfo, Handle* outHandle ) override;
	RenderStatus CreateShader( ShaderInfo_t pipelineInfo, Handle* outHandle ) override;

public:
	// All vulkan types should be able to access render context internals.
	// This saves us having to pass things like m_device around whenever we want
	// to do anything
	friend VulkanObject;
	friend VulkanSwapchain;
	friend VulkanBuffer;
	friend VulkanSampler;
	friend VulkanCommandContext;
	friend VulkanImageTexture;
	friend VulkanRenderTexture;
	friend VulkanDescriptor;
	friend VulkanPipeline;
	friend VulkanShader;

	// ----------------------------------------

	RenderStatus Startup() override;
	RenderStatus Shutdown() override;
	RenderStatus BeginRendering() override;
	RenderStatus EndRendering() override;

	// ----------------------------------------

	RenderStatus BindPipeline( Pipeline p ) override;

	RenderStatus BindDescriptor( Descriptor d ) override;

	RenderStatus UpdateDescriptor( Descriptor d, DescriptorUpdateInfo_t updateInfo ) override;

	RenderStatus BindVertexBuffer( VertexBuffer vb ) override;

	RenderStatus BindIndexBuffer( IndexBuffer ib ) override;

	RenderStatus BindConstants( RenderPushConstants p ) override;

	RenderStatus Draw( uint32_t vertexCount, uint32_t indexCount, uint32_t instanceCount ) override;

	RenderStatus BindRenderTarget( RenderTexture rt ) override;

	RenderStatus GetRenderSize( Size2D* outSize ) override;

	// ----------------------------------------

	RenderStatus RenderMesh( RenderPushConstants constants, Mesh* mesh ) override;
};
