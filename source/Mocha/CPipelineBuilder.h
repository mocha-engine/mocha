#pragma once

#include "CShader.h"
#include "Mocha.h"

#include <vector>
#include <vulkan/vulkan.hpp>

class CPipelineBuilder
{
private:
	std::vector<VkPipelineShaderStageCreateInfo> CreatePipelineShaderStageCreateInfo( CShader* shader );
	VkPipelineVertexInputStateCreateInfo CreateVertexInputStateCreateInfo();
	VkPipelineInputAssemblyStateCreateInfo CreateInputAssemblyStateCreateInfo();
	VkPipelineRasterizationStateCreateInfo CreateRasterizationStateCreateInfo();
	VkPipelineMultisampleStateCreateInfo CreateMultisampleStateCreateInfo();
	VkPipelineColorBlendAttachmentState CreateColorBlendAttachmentState();
	VkPipelineLayoutCreateInfo CreateLayoutCreateInfo();

	void CreateLayout();
	
	std::vector<VkPipelineShaderStageCreateInfo> mShaderStages;
	VkPipelineVertexInputStateCreateInfo mVertexInputInfo;
	VkPipelineInputAssemblyStateCreateInfo mInputAssembly;

	VkViewport mViewport;
	VkRect2D mScissor;

	VkPipelineRasterizationStateCreateInfo mRasterizer;
	VkPipelineColorBlendAttachmentState mColorBlendAttachment;
	VkPipelineMultisampleStateCreateInfo mMultisampling;

	VkPipelineLayout mPipelineLayout;
	
public:
	VkPipeline BuildPipeline( VkDevice device, VkRenderPass renderPass, CShader* shader );
};
