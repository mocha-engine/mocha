#pragma once
#include <texture.h>
#include <vector>
#include <vulkan/types.h>

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
	void CreatePipeline();
	void CreateDescriptors();

	void CreateResources();
	bool LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

	size_t GetSizeOf( VertexAttributeFormat format );
	VertexInputDescription CreateVertexDescription( std::vector<VertexAttribute> vertexAttributes );
	VkFormat GetVulkanFormat( VertexAttributeFormat format );

public:
	std::vector<Texture> m_textures;
	std::string m_shaderPath;

	VkDescriptorSet m_textureSet;
	VkDescriptorSetLayout m_textureSetLayout;
	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;

	VertexInputDescription m_vertexInputDescription;

	Material( const char* shaderPath, InteropStruct vertexAttributes, InteropStruct textures );
};