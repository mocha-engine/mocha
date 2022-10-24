#include "CPipelineBuilder.h"

std::vector<VkPipelineShaderStageCreateInfo> CPipelineBuilder::CreatePipelineShaderStageCreateInfo( CShader* shader )
{
	std::vector<VkPipelineShaderStageCreateInfo> shaderStages = {};

	{
		VkPipelineShaderStageCreateInfo info = {};
		info.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		info.pNext = nullptr;

		info.stage = VK_SHADER_STAGE_VERTEX_BIT;
		info.module = shader->mVertexModule;
		info.pName = "main";

		shaderStages.push_back( info );
	}

	{
		VkPipelineShaderStageCreateInfo info = {};
		info.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		info.pNext = nullptr;

		info.stage = VK_SHADER_STAGE_FRAGMENT_BIT;
		info.module = shader->mFragmentModule;
		info.pName = "main";

		shaderStages.push_back( info );
	}

	return shaderStages;
}

VkPipelineVertexInputStateCreateInfo CPipelineBuilder::CreateVertexInputStateCreateInfo()
{
	VkPipelineVertexInputStateCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
	info.pNext = nullptr;

	info.vertexBindingDescriptionCount = 0;
	info.vertexAttributeDescriptionCount = 0;

	return info;	
}

VkPipelineInputAssemblyStateCreateInfo CPipelineBuilder::CreateInputAssemblyStateCreateInfo()
{
	VkPipelineInputAssemblyStateCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
	info.pNext = nullptr;
	
	info.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
	info.primitiveRestartEnable = VK_FALSE;

	return info;
}

VkPipelineRasterizationStateCreateInfo CPipelineBuilder::CreateRasterizationStateCreateInfo()
{
	VkPipelineRasterizationStateCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
	info.pNext = nullptr;

	info.depthClampEnable = VK_FALSE;
	info.rasterizerDiscardEnable = VK_FALSE;
	
	info.polygonMode = VK_POLYGON_MODE_FILL;
	info.lineWidth = 1.0f;
	
	info.cullMode = VK_CULL_MODE_NONE;
	info.frontFace = VK_FRONT_FACE_CLOCKWISE;
	
	info.depthBiasEnable = VK_FALSE;
	info.depthBiasConstantFactor = 0.0f;
	info.depthBiasClamp = 0.0f;
	info.depthBiasSlopeFactor = 0.0f;

	return info;
}

VkPipelineMultisampleStateCreateInfo CPipelineBuilder::CreateMultisampleStateCreateInfo()
{
	VkPipelineMultisampleStateCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
	info.pNext = nullptr;

	info.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;
	info.sampleShadingEnable = VK_FALSE;
	info.minSampleShading = 1.0f;
	info.pSampleMask = nullptr;
	info.alphaToCoverageEnable = VK_FALSE;
	info.alphaToOneEnable = VK_FALSE;

	return info;
}

VkPipelineColorBlendAttachmentState CPipelineBuilder::CreateColorBlendAttachmentState()
{
	VkPipelineColorBlendAttachmentState colorBlendAttachment = {};
	
	colorBlendAttachment.colorWriteMask =
	    VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
	colorBlendAttachment.blendEnable = VK_FALSE;
	
	return colorBlendAttachment;	
}

VkPipelineLayoutCreateInfo CPipelineBuilder::CreateLayoutCreateInfo()
{
	VkPipelineLayoutCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	info.pNext = nullptr;

	info.flags = 0;
	info.setLayoutCount = 0;
	info.pSetLayouts = nullptr;
	info.pushConstantRangeCount = 0;
	info.pPushConstantRanges = nullptr;

	return info;
}

void CPipelineBuilder::CreateLayout()
{
	VkPipelineLayoutCreateInfo pipelineLayoutInfo = CreateLayoutCreateInfo();
	
	ASSERT( vkCreatePipelineLayout( *g_Device, &pipelineLayoutInfo, nullptr, &mPipelineLayout ) );
}

VkPipeline CPipelineBuilder::BuildPipeline( VkDevice device, VkRenderPass renderPass, CShader* shader )
{
	VkPipelineViewportStateCreateInfo viewportState = {};
	viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
	viewportState.pNext = nullptr;

	//
	//
	//

	mViewport.x = 0;
	mViewport.y = 0;
	mViewport.width = 1280;
	mViewport.height = 720;
	mViewport.minDepth = 0.0f;
	mViewport.maxDepth = 1.0f;

	mScissor.offset = { 0, 0 };
	mScissor.extent = { 1280, 720 };

	//
	//
	//

	viewportState.viewportCount = 1;
	viewportState.scissorCount = 1;
	viewportState.pScissors = &mScissor;
	viewportState.pViewports = &mViewport;

	VkPipelineColorBlendStateCreateInfo colorBlendState = {};
	colorBlendState.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	colorBlendState.pNext = nullptr;
	
	colorBlendState.attachmentCount = 1;
	colorBlendState.pAttachments = &mColorBlendAttachment;
	colorBlendState.logicOpEnable = VK_FALSE;
	colorBlendState.logicOp = VK_LOGIC_OP_COPY;

	VkGraphicsPipelineCreateInfo pipelineInfo = {};
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineInfo.pNext = nullptr;

	//
	//
	//

	mShaderStages = CreatePipelineShaderStageCreateInfo( shader );
	mVertexInputInfo = CreateVertexInputStateCreateInfo();
	mInputAssembly = CreateInputAssemblyStateCreateInfo();
	mRasterizer = CreateRasterizationStateCreateInfo();
	mColorBlendAttachment = CreateColorBlendAttachmentState();
	mMultisampling = CreateMultisampleStateCreateInfo();

	CreateLayout();
	
	//
	//
	//
	pipelineInfo.stageCount = mShaderStages.size();
	pipelineInfo.pStages = mShaderStages.data();
	pipelineInfo.pVertexInputState = &mVertexInputInfo;
	pipelineInfo.pInputAssemblyState = &mInputAssembly;
	pipelineInfo.pViewportState = &viewportState;
	pipelineInfo.pRasterizationState = &mRasterizer;
	pipelineInfo.pMultisampleState = &mMultisampling;
	pipelineInfo.pColorBlendState = &colorBlendState;

	pipelineInfo.layout = mPipelineLayout;
	pipelineInfo.renderPass = renderPass;
	pipelineInfo.subpass = 0;

	pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
	
	VkPipeline pipeline;
	ASSERT( vkCreateGraphicsPipelines( device, VK_NULL_HANDLE, 1, &pipelineInfo, nullptr, &pipeline ) );
	return pipeline;
}
