#pragma once
#include <mesh.h>
#include <vector>
#include <vk_types.h>
#include <vkinit.h>

class PipelineBuilder
{
public:
	std::vector<VkPipelineShaderStageCreateInfo> m_shaderStages = {};
	VkPipelineVertexInputStateCreateInfo m_vertexInputInfo = {};
	VkPipelineInputAssemblyStateCreateInfo m_inputAssembly = {};

	VkPipelineRasterizationStateCreateInfo m_rasterizer = {};
	VkPipelineColorBlendAttachmentState m_colorBlendAttachment = {};
	VkPipelineMultisampleStateCreateInfo m_multisampling = {};
	VkPipelineLayout m_pipelineLayout = {};

	VkPipelineDepthStencilStateCreateInfo m_depthStencil = {};

	VkPipeline Build( VkDevice device, VkFormat depthFormat, VkFormat colorFormat );
};

class PipelineFactory
{
private:
	std::vector<VkPipelineShaderStageCreateInfo> m_shaderStages;

	VkPolygonMode m_fillMode = VK_POLYGON_MODE_FILL;
	VkCullModeFlags m_cullMode = VK_CULL_MODE_NONE;
	bool m_depthRead = true;
	bool m_depthWrite = true;

	VkPipelineLayout m_layout;

	VertexInputDescription m_vertexDescription;

public:
	inline static PipelineFactory begin() { return PipelineFactory(); }

	inline PipelineFactory() { m_layout = {}; }

	inline PipelineFactory WithFragmentShader( VkShaderModule fragmentShader )
	{
		m_shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader ) );
		return *this;
	}

	inline PipelineFactory WithVertexShader( VkShaderModule vertexShader )
	{
		m_shaderStages.push_back( VKInit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_VERTEX_BIT, vertexShader ) );
		return *this;
	}

	inline PipelineFactory WithVertexDescription( VertexInputDescription vertexDescription )
	{
		m_vertexDescription = vertexDescription;
		return *this;
	}

	inline PipelineFactory WithFillMode( VkPolygonMode fillMode )
	{
		m_fillMode = fillMode;
		return *this;
	}

	inline PipelineFactory WithCullMode( VkCullModeFlags cullMode )
	{
		m_cullMode = cullMode;
		return *this;
	}

	inline PipelineFactory WithLayout( VkPipelineLayout layout )
	{
		m_layout = layout;
		return *this;
	}

	inline PipelineFactory WithDepthInfo( bool depthRead, bool depthWrite )
	{
		m_depthRead = depthRead;
		m_depthWrite = depthWrite;
		return *this;
	}

	inline VkPipeline Build( VkDevice device, VkFormat colorFormat, VkFormat depthFormat )
	{
		PipelineBuilder builder;

		builder.m_pipelineLayout = m_layout;

		builder.m_rasterizer = VKInit::PipelineRasterizationStateCreateInfo( m_fillMode );
		builder.m_multisampling = VKInit::PipelineMultisampleStateCreateInfo();
		builder.m_colorBlendAttachment = VKInit::PipelineColorBlendAttachmentState();

		builder.m_shaderStages = m_shaderStages;

		builder.m_vertexInputInfo = VKInit::PipelineVertexInputStateCreateInfo();
		builder.m_vertexInputInfo.pVertexAttributeDescriptions = m_vertexDescription.attributes.data();
		builder.m_vertexInputInfo.vertexAttributeDescriptionCount =
		    static_cast<uint32_t>( m_vertexDescription.attributes.size() );

		builder.m_vertexInputInfo.pVertexBindingDescriptions = m_vertexDescription.bindings.data();
		builder.m_vertexInputInfo.vertexBindingDescriptionCount = static_cast<uint32_t>( m_vertexDescription.bindings.size() );

		builder.m_inputAssembly = VKInit::PipelineInputAssemblyStateCreateInfo( VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST );
		builder.m_depthStencil = VKInit::DepthStencilCreateInfo( m_depthRead, m_depthWrite, VK_COMPARE_OP_LESS_OR_EQUAL );

		return builder.Build( device, colorFormat, depthFormat );
	}
};