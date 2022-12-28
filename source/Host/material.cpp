#include "material.h"

#include <globalvars.h>
#include <model.h>
#include <rendering.h>
#include <rendermanager.h>
#include <vkinit.h>

#if RAYTRACING
void Material::CreateAccelDescriptors()
{
	VkDevice device = g_renderManager->m_device;
	VkDescriptorPool descriptorPool = g_renderManager->m_descriptorPool;

	VkDescriptorSetLayoutBinding accelerationStructureLayoutBinding = {};
	accelerationStructureLayoutBinding.binding = 0;
	accelerationStructureLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_KHR;
	accelerationStructureLayoutBinding.descriptorCount = 1;
	accelerationStructureLayoutBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

	VkDescriptorSetLayoutCreateInfo accelerationStructureLayoutInfo =
	    VKInit::DescriptorSetLayoutCreateInfo( &accelerationStructureLayoutBinding, 1 );
	VK_CHECK(
	    vkCreateDescriptorSetLayout( device, &accelerationStructureLayoutInfo, nullptr, &m_accelerationStructureSetLayout ) );

	VkDescriptorSetAllocateInfo accelerationStructureAllocInfo =
	    VKInit::DescriptorSetAllocateInfo( descriptorPool, &m_accelerationStructureSetLayout, 1 );

	VK_CHECK( vkAllocateDescriptorSets( device, &accelerationStructureAllocInfo, &m_accelerationStructureSet ) );

	VkWriteDescriptorSetAccelerationStructureKHR accelerationStructureWrite = {};
	accelerationStructureWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET_ACCELERATION_STRUCTURE_KHR;
	accelerationStructureWrite.accelerationStructureCount = 1;
	accelerationStructureWrite.pAccelerationStructures = &g_renderManager->m_tlas.accel;

	VkWriteDescriptorSet accelerationStructureDescriptorWrite = {};
	accelerationStructureDescriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
	accelerationStructureDescriptorWrite.dstSet = m_accelerationStructureSet;
	accelerationStructureDescriptorWrite.dstBinding = 0;
	accelerationStructureDescriptorWrite.descriptorCount = 1;
	accelerationStructureDescriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_KHR;
	accelerationStructureDescriptorWrite.pNext = &accelerationStructureWrite;

	vkUpdateDescriptorSets( device, 1, &accelerationStructureDescriptorWrite, 0, nullptr );
}
#endif

Material::Material(
    const char* shaderPath, InteropArray vertexAttributes, InteropArray textures, Sampler sampler, bool ignoreDepth )
{
	auto texturePtrs = textures.GetData<Texture*>();
	m_textures = std::vector<Texture>( textures.count );

	for ( int i = 0; i < textures.count; i++ )
	{
		m_textures[i] = Texture( *texturePtrs[i] );
	}

	m_shaderPath = std::string( shaderPath );

	auto attributes = vertexAttributes.GetData<VertexAttribute>();
	m_vertexInputDescription = CreateVertexDescription( attributes );

	m_sampler = sampler;
	m_ignoreDepth = ignoreDepth;
}

void Material::CreateDescriptors()
{
	VkDevice device = g_renderManager->m_device;
	VkDescriptorPool descriptorPool = g_renderManager->m_descriptorPool;

	std::vector<VkDescriptorSetLayoutBinding> textureBindings = {};

	for ( int i = 0; i < m_textures.size(); ++i )
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

	VkDescriptorSetAllocateInfo allocInfo = VKInit::DescriptorSetAllocateInfo( descriptorPool, &m_textureSetLayout, 1 );

	VK_CHECK( vkAllocateDescriptorSets( device, &allocInfo, &m_textureSet ) );

	std::vector<VkDescriptorImageInfo> imageInfos = {};
	std::vector<VkWriteDescriptorSet> descriptorWrites = {};

	for ( int i = 0; i < m_textures.size(); ++i )
	{
		Texture texture = m_textures[i];

		VkDescriptorImageInfo imageInfo = {};
		imageInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
		imageInfo.imageView = texture.GetImageView();
		imageInfo.sampler =
		    m_sampler == Sampler::Anisotropic ? g_renderManager->m_anisoSampler : g_renderManager->m_pointSampler;

		imageInfos.push_back( imageInfo );
	}

	for ( int i = 0; i < m_textures.size(); ++i )
		descriptorWrites.push_back(
		    VKInit::WriteDescriptorImage( VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER, m_textureSet, &imageInfos[i], i ) );

	vkUpdateDescriptorSets( device, descriptorWrites.size(), descriptorWrites.data(), 0, nullptr );
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

#if RAYTRACING
	setLayouts.push_back( m_accelerationStructureSetLayout );
#endif

	pipeline_layout_info.pSetLayouts = setLayouts.data();
	pipeline_layout_info.setLayoutCount = setLayouts.size();

	VK_CHECK( vkCreatePipelineLayout( device, &pipeline_layout_info, nullptr, &m_pipelineLayout ) );

	m_pipeline = PipelineFactory::begin()
	                 .WithFragmentShader( triangleFragShader )
	                 .WithVertexShader( triangleVertexShader )
	                 .WithVertexDescription( m_vertexInputDescription )
	                 .WithLayout( m_pipelineLayout )
	                 .WithDepthInfo( !m_ignoreDepth, !m_ignoreDepth )
	                 .Build( device, colorFormat, depthFormat );
}

void Material::CreateResources()
{
	CreateDescriptors();

#if RAYTRACING
	CreateAccelDescriptors();
#endif

	CreatePipeline();
}

void Material::ReloadShaders()
{
	CreateResources();
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
	if ( !ShaderCompiler::Instance().Compile( shaderStage, buffer, shaderBits ) )
	{
		std::string error = std::string( filePath ) + " failed to compile.\nCheck the console for more details.";
		ERRORMESSAGE( error );
		exit( 1 );
	}

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

VkFormat Material::GetVulkanFormat( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VertexAttributeFormat::Int:
		return VK_FORMAT_R32_SINT;
		break;
	case VertexAttributeFormat::Float:
		return VK_FORMAT_R32_SFLOAT;
		break;
	case VertexAttributeFormat::Float2:
		return VK_FORMAT_R32G32_SFLOAT;
		break;
	case VertexAttributeFormat::Float3:
		return VK_FORMAT_R32G32B32_SFLOAT;
		break;
	case VertexAttributeFormat::Float4:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
		break;
	}

	return VK_FORMAT_UNDEFINED;
}

size_t Material::GetSizeOf( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VertexAttributeFormat::Int:
		return sizeof( int );
		break;
	case VertexAttributeFormat::Float:
		return sizeof( float );
		break;
	case VertexAttributeFormat::Float2:
		return sizeof( float ) * 2;
		break;
	case VertexAttributeFormat::Float3:
		return sizeof( float ) * 3;
		break;
	case VertexAttributeFormat::Float4:
		return sizeof( float ) * 4;
		break;
	}

	return 0;
}

VertexInputDescription Material::CreateVertexDescription( std::vector<VertexAttribute> vertexAttributes )
{
	// Calculate stride size
	size_t stride = 0;
	for ( int i = 0; i < vertexAttributes.size(); ++i )
	{
		stride += GetSizeOf( ( VertexAttributeFormat )vertexAttributes[i].format );
	}

	VertexInputDescription description = {};

	VkVertexInputBindingDescription mainBinding = {};
	mainBinding.binding = 0;
	mainBinding.stride = stride;
	mainBinding.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

	description.bindings.push_back( mainBinding );

	size_t offset = 0;

	for ( int i = 0; i < vertexAttributes.size(); ++i )
	{
		auto attribute = vertexAttributes[i];

		VkVertexInputAttributeDescription positionAttribute = {};
		positionAttribute.binding = 0;
		positionAttribute.location = i;
		positionAttribute.format = GetVulkanFormat( ( VertexAttributeFormat )attribute.format );
		positionAttribute.offset = offset;
		description.attributes.push_back( positionAttribute );

		offset += GetSizeOf( ( VertexAttributeFormat )attribute.format );
	}

	return description;
}