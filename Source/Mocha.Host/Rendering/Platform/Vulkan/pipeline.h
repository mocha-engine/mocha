#pragma once
#include <Rendering/Platform/Vulkan/vkinit.h>
#include <vector>

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