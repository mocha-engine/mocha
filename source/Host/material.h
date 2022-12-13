#pragma once
#include <texture.h>
#include <vulkan/types.h>

class Material
{
private:
	void CreatePipeline();
	void CreateDescriptors();

	void CreateResources();
	bool LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

public:
	Texture m_diffuseTexture;
	VkDescriptorSet m_textureSet;
	VkDescriptorSetLayout m_textureSetLayout;
	VkSampler m_sampler; // TODO: Should probably just have a handful of global samplers
	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;

	// This is defined for shit reasons
	// TODO: get rid
	Material(){};

	Material( Texture diffuseTexture );
};
