#pragma once
#include <defs.h>
#include <texture.h>
#include <vector>
#include <vk_types.h>

enum Sampler
{
	Anisotropic,
	Point
};

enum VertexAttributeFormat
{
	Int,
	Float,
	Float2,
	Float3,
	Float4
};

struct VertexAttribute
{
	const char* name;
	int format;
};

struct VertexInputDescription
{
	std::vector<VkVertexInputBindingDescription> bindings;
	std::vector<VkVertexInputAttributeDescription> attributes;

	VkPipelineVertexInputStateCreateFlags flags = 0;
};

//@InteropGen generate class
class Material
{
private:
	void CreateDescriptors();

	bool LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );
	void CreatePipeline();
	void CreateAccelDescriptors();

	size_t GetSizeOf( VertexAttributeFormat format );
	VertexInputDescription CreateVertexDescription( std::vector<VertexAttribute> vertexAttributes );
	VkFormat GetVulkanFormat( VertexAttributeFormat format );

public:
	std::vector<Texture> m_textures;
	std::string m_shaderPath;
	
	void CreateResources();
	void ReloadShaders();

	VkDescriptorSet m_textureSet;
	VkDescriptorSetLayout m_textureSetLayout;

	VkDescriptorSet m_accelerationStructureSet;
	VkDescriptorSetLayout m_accelerationStructureSetLayout;

	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;
	Sampler m_sampler;
	bool m_ignoreDepth;

	VertexInputDescription m_vertexInputDescription;

	Material( const char* shaderPath, InteropArray vertexAttributes, InteropArray textures, Sampler sampler, bool ignoreDepth );
};