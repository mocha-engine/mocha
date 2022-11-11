#include "vk_pipeline.h"

#include <spdlog/spdlog.h>

VkPipeline PipelineBuilder::Build( VkDevice device, VkFormat colorRenderingFormat )
{
	VkPipelineViewportStateCreateInfo viewportState = {};
	viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
	viewportState.pNext = nullptr;

	viewportState.viewportCount = 1;
	viewportState.pViewports = &m_viewport;
	viewportState.scissorCount = 1;
	viewportState.pScissors = &m_scissor;

	VkPipelineColorBlendStateCreateInfo colorBlending = {};
	colorBlending.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	colorBlending.pNext = nullptr;

	colorBlending.logicOpEnable = VK_FALSE;
	colorBlending.logicOp = VK_LOGIC_OP_COPY;
	colorBlending.attachmentCount = 1;
	colorBlending.pAttachments = &m_colorBlendAttachment;

	VkPipelineRenderingCreateInfo pipeline_create = {};
	pipeline_create.sType = VK_STRUCTURE_TYPE_PIPELINE_RENDERING_CREATE_INFO;
	pipeline_create.pNext = VK_NULL_HANDLE;
	pipeline_create.colorAttachmentCount = 1;
	pipeline_create.pColorAttachmentFormats = &colorRenderingFormat;

	VkGraphicsPipelineCreateInfo pipelineInfo = {};
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineInfo.pNext = &pipeline_create;

	pipelineInfo.stageCount = static_cast<uint32_t>( m_shaderStages.size() );
	pipelineInfo.pStages = m_shaderStages.data();
	pipelineInfo.pVertexInputState = &m_vertexInputInfo;
	pipelineInfo.pInputAssemblyState = &m_inputAssembly;
	pipelineInfo.pViewportState = &viewportState;
	pipelineInfo.pRasterizationState = &m_rasterizer;
	pipelineInfo.pMultisampleState = &m_multisampling;
	pipelineInfo.pColorBlendState = &colorBlending;
	pipelineInfo.layout = m_pipelineLayout;
	pipelineInfo.subpass = 0;
	pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
	pipelineInfo.renderPass = VK_NULL_HANDLE;

	VkPipeline newPipeline;
	if ( vkCreateGraphicsPipelines( device, VK_NULL_HANDLE, 1, &pipelineInfo, nullptr, &newPipeline ) != VK_SUCCESS )
	{
		spdlog::error( "Failed to create graphics pipeline" );
		return VK_NULL_HANDLE;
	}
	else
	{
		return newPipeline;
	}
}
