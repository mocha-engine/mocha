#include "material.h"

#include <game/model.h>
#include <globalvars.h>
#include <vulkan/rendermanager.h>
#include <vulkan/vkinit.h>

Material::Material( std::string shaderPath, VertexInputDescription vertexInputDescription, Texture diffuseTexture,
    Texture normalTexture, Texture ambientOcclusionTexture, Texture metalnessTexture, Texture roughnessTexture )
{
	m_shaderPath = shaderPath;
	m_vertexInputDescription = vertexInputDescription;

	m_diffuseTexture = diffuseTexture;
	m_normalTexture = normalTexture;
	m_ambientOcclusionTexture = ambientOcclusionTexture;
	m_metalnessTexture = metalnessTexture;
	m_roughnessTexture = roughnessTexture;

	CreateResources();
}

void Material::CreateDescriptors()
{
	VkDevice device = g_renderManager->m_device;
	VkDescriptorPool descriptorPool = g_renderManager->m_descriptorPool;

	std::vector<Texture> textures = {
	    m_diffuseTexture, m_normalTexture, m_ambientOcclusionTexture, m_metalnessTexture, m_roughnessTexture };

	std::vector<VkDescriptorSetLayoutBinding> textureBindings = {};

	for ( int i = 0; i < textures.size(); ++i )
	{
		VkDescriptorSetLayoutBinding textureBinding = {};
		textureBinding.binding = i;
		textureBinding.descriptorType = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
		textureBinding.descriptorCount = 1;
		textureBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

		textureBindings.push_back( textureBinding );
	}

	VkDescriptorSetLayoutCreateInfo textureLayoutInfo =
	    VKInit::DescriptorSetLayoutCreateInfo( textureBindings.data(), textureBindings.size() );
	VK_CHECK( vkCreateDescriptorSetLayout( device, &textureLayoutInfo, nullptr, &m_textureSetLayout ) );

	VkDescriptorSetAllocateInfo allocInfo =
	    VKInit::DescriptorSetAllocateInfo( g_renderManager->m_descriptorPool, &m_textureSetLayout, 1 );

	VK_CHECK( vkAllocateDescriptorSets( g_renderManager->m_device, &allocInfo, &m_textureSet ) );

	std::vector<VkDescriptorImageInfo> imageInfos = {};
	std::vector<VkWriteDescriptorSet> descriptorWrites = {};

	for ( int i = 0; i < textures.size(); ++i )
	{
		Texture texture = textures[i];

		VkDescriptorImageInfo imageInfo = {};
		imageInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
		imageInfo.imageView = textures[i].GetImageView();
		imageInfo.sampler = g_renderManager->m_pointSampler;

		imageInfos.push_back( imageInfo );
	}

	for ( int i = 0; i < textures.size(); ++i )
		descriptorWrites.push_back(
		    VKInit::WriteDescriptorImage( VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, m_textureSet, &imageInfos[i], i ) );

	vkUpdateDescriptorSets( g_renderManager->m_device, descriptorWrites.size(), descriptorWrites.data(), 0, nullptr );
}

void Material::CreatePipeline()
{
	VkDevice device = g_renderManager->m_device;
	VkExtent2D windowExtent = g_renderManager->GetWindowExtent();
	VkFormat colorFormat = g_renderManager->m_swapchainImageFormat;
	VkFormat depthFormat = g_renderManager->m_depthFormat;

	VkShaderModule triangleFragShader;
	if ( LoadShaderModule( m_shaderPath.c_str(), VK_SHADER_STAGE_FRAGMENT_BIT, &triangleFragShader ) )
		spdlog::info( "Frag shader compiled successfully" );

	VkShaderModule triangleVertexShader;
	if ( LoadShaderModule( m_shaderPath.c_str(), VK_SHADER_STAGE_VERTEX_BIT, &triangleVertexShader ) )
		spdlog::info( "Vert shader compiled successfully" );

	VkPipelineLayoutCreateInfo pipeline_layout_info = VKInit::PipelineLayoutCreateInfo();
	VkPushConstantRange push_constant = {};

	push_constant.offset = 0;
	push_constant.size = sizeof( MeshPushConstants );
	push_constant.stageFlags = VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT;

	pipeline_layout_info.pPushConstantRanges = &push_constant;
	pipeline_layout_info.pushConstantRangeCount = 1;

	std::vector<VkDescriptorSetLayout> setLayouts;
	setLayouts.push_back( m_textureSetLayout );

	pipeline_layout_info.pSetLayouts = setLayouts.data();
	pipeline_layout_info.setLayoutCount = setLayouts.size();

	VK_CHECK( vkCreatePipelineLayout( device, &pipeline_layout_info, nullptr, &m_pipelineLayout ) );

	m_pipeline = PipelineFactory::begin()
	                 .WithFragmentShader( triangleFragShader )
	                 .WithVertexShader( triangleVertexShader )
	                 .WithVertexDescription( m_vertexInputDescription )
	                 .WithLayout( m_pipelineLayout )
	                 .Build( device, colorFormat, depthFormat );
}

void Material::CreateResources()
{
	CreateDescriptors();
	CreatePipeline();
}

bool Material::LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule )
{
	VkDevice device = g_renderManager->m_device;

	std::string line, text;
	std::ifstream in( filePath );

	while ( std::getline( in, line ) )
	{
		text += line + "\n";
	}

	const char* buffer = text.c_str();

	std::vector<unsigned int> shaderBits;
	ShaderCompiler::Instance().Compile( shaderStage, buffer, shaderBits );

	//
	//
	//

	VkShaderModuleCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.pNext = nullptr;

	createInfo.codeSize = shaderBits.size() * sizeof( uint32_t );
	createInfo.pCode = shaderBits.data();

	VkShaderModule shaderModule;

	if ( vkCreateShaderModule( device, &createInfo, nullptr, &shaderModule ) != VK_SUCCESS )
	{
		spdlog::error( "Could not compile shader {}", filePath );
		return false;
	}

	*outShaderModule = shaderModule;
	return true;
}
