#pragma once
#include <vector>
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
	Texture m_normalTexture;
	Texture m_ambientOcclusionTexture;
	Texture m_metalnessTexture;
	Texture m_roughnessTexture;

	VkDescriptorSet m_textureSet;
	VkDescriptorSetLayout m_textureSetLayout;
	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;
	
	Material( Texture diffuseTexture, Texture normalTexture, Texture ambientOcclusionTexture, Texture metalnessTexture,
	    Texture roughnessTexture );
};