#pragma once
#include "vk_initializers.h"
#include "vk_mesh.h"
#include "vk_types.h"

#include <vector>

class PipelineBuilder
{
public:
	std::vector<VkPipelineShaderStageCreateInfo> m_shaderStages = {};
	VkPipelineVertexInputStateCreateInfo m_vertexInputInfo = {};
	VkPipelineInputAssemblyStateCreateInfo m_inputAssembly = {};

	VkViewport m_viewport = {};
	VkRect2D m_scissor = {};

	VkPipelineRasterizationStateCreateInfo m_rasterizer = {};
	VkPipelineColorBlendAttachmentState m_colorBlendAttachment = {};
	VkPipelineMultisampleStateCreateInfo m_multisampling = {};
	VkPipelineLayout m_pipelineLayout = {};

	VkPipeline Build( VkDevice device, VkFormat colorRenderingFormat );
};

class PipelineFactory
{
private:
	std::vector<VkPipelineShaderStageCreateInfo> m_shaderStages;

	VkPolygonMode m_fillMode = VK_POLYGON_MODE_FILL;
	VkCullModeFlags m_cullMode = VK_CULL_MODE_NONE;

	VkViewport m_viewport;
	VkRect2D m_scissor = VkRect2D{ { 0, 0 }, { 1280, 720 } };

	VkPipelineLayout m_layout;

	VertexInputDescription m_vertexDescription;

public:
	inline static PipelineFactory begin() { return PipelineFactory(); }

	inline PipelineFactory()
	{
		m_layout = {};

		m_viewport.x = 0.0f;
		m_viewport.y = 0.0f;
		m_viewport.width = 1280.0f;
		m_viewport.height = 720.0f;
		m_viewport.minDepth = 0.0f;
		m_viewport.maxDepth = 1.0f;
	}

	inline PipelineFactory WithFragmentShader( VkShaderModule fragmentShader )
	{
		m_shaderStages.push_back( vkinit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader ) );
		return *this;
	}

	inline PipelineFactory WithVertexShader( VkShaderModule vertexShader )
	{
		m_shaderStages.push_back( vkinit::PipelineShaderStageCreateInfo( VK_SHADER_STAGE_VERTEX_BIT, vertexShader ) );
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

	inline PipelineFactory WithViewport( VkViewport viewport )
	{
		m_viewport = viewport;
		return *this;
	}

	inline PipelineFactory WithScissor( VkRect2D scissor )
	{
		m_scissor = scissor;
		return *this;
	}

	inline PipelineFactory WithLayout( VkPipelineLayout layout )
	{
		m_layout = layout;
		return *this;
	}

	inline VkPipeline Build( VkDevice device, VkFormat swapchainImageFormat )
	{
		PipelineBuilder builder;

		builder.m_viewport = m_viewport;
		builder.m_scissor = m_scissor;
		builder.m_pipelineLayout = m_layout;

		builder.m_rasterizer = vkinit::PipelineRasterizationStateCreateInfo( m_fillMode );
		builder.m_multisampling = vkinit::PipelineMultisampleStateCreateInfo();
		builder.m_colorBlendAttachment = vkinit::PipelineColorBlendAttachmentState();

		builder.m_shaderStages = m_shaderStages;

		builder.m_vertexInputInfo = vkinit::PipelineVertexInputStateCreateInfo();
		builder.m_vertexInputInfo.pVertexAttributeDescriptions = m_vertexDescription.attributes.data();
		builder.m_vertexInputInfo.vertexAttributeDescriptionCount =
		    static_cast<uint32_t>( m_vertexDescription.attributes.size() );

		builder.m_vertexInputInfo.pVertexBindingDescriptions = m_vertexDescription.bindings.data();
		builder.m_vertexInputInfo.vertexBindingDescriptionCount = static_cast<uint32_t>( m_vertexDescription.bindings.size() );

		builder.m_inputAssembly = vkinit::PipelineInputAssemblyStateCreateInfo( VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST );

		return builder.Build( device, swapchainImageFormat );
	}
};