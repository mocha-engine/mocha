#pragma once
#include <Misc/mathtypes.h>
#include <Rendering/Platform/Vulkan/vkmacros.h>

namespace VKInit
{
	inline VkPipelineShaderStageCreateInfo PipelineShaderStageCreateInfo(
	    VkShaderStageFlagBits stage, VkShaderModule shaderModule )
	{
		VkPipelineShaderStageCreateInfo shaderStageInfo{};
		shaderStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStageInfo.pNext = nullptr;

		shaderStageInfo.stage = stage;
		shaderStageInfo.module = shaderModule;
		shaderStageInfo.pName = "main";

		return shaderStageInfo;
	}

	inline VkPipelineVertexInputStateCreateInfo PipelineVertexInputStateCreateInfo()
	{
		VkPipelineVertexInputStateCreateInfo vertexInputInfo{};
		vertexInputInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
		vertexInputInfo.pNext = nullptr;

		vertexInputInfo.vertexBindingDescriptionCount = 0;
		vertexInputInfo.vertexAttributeDescriptionCount = 0;

		return vertexInputInfo;
	}

	inline VkPipelineInputAssemblyStateCreateInfo PipelineInputAssemblyStateCreateInfo( VkPrimitiveTopology topology )
	{
		VkPipelineInputAssemblyStateCreateInfo inputAssembly{};
		inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
		inputAssembly.pNext = nullptr;

		inputAssembly.topology = topology;
		inputAssembly.primitiveRestartEnable = VK_FALSE;

		return inputAssembly;
	}

	inline VkPipelineRasterizationStateCreateInfo PipelineRasterizationStateCreateInfo( VkPolygonMode polygonMode )
	{
		VkPipelineRasterizationStateCreateInfo rasterizer{};
		rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
		rasterizer.pNext = nullptr;

		rasterizer.depthClampEnable = VK_FALSE;
		rasterizer.rasterizerDiscardEnable = VK_FALSE;
		rasterizer.polygonMode = polygonMode;
		rasterizer.lineWidth = 1.0f;
		rasterizer.cullMode = VK_CULL_MODE_BACK_BIT;
		rasterizer.frontFace = VK_FRONT_FACE_CLOCKWISE;
		rasterizer.depthBiasEnable = VK_FALSE;
		rasterizer.depthBiasConstantFactor = 0.0f;
		rasterizer.depthBiasClamp = 0.0f;
		rasterizer.depthBiasSlopeFactor = 0.0f;

		return rasterizer;
	}

	inline VkPipelineMultisampleStateCreateInfo PipelineMultisampleStateCreateInfo()
	{
		VkPipelineMultisampleStateCreateInfo multisampling{};
		multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
		multisampling.pNext = nullptr;

		multisampling.sampleShadingEnable = VK_FALSE;
		multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;
		multisampling.minSampleShading = 1.0f;
		multisampling.pSampleMask = nullptr;
		multisampling.alphaToCoverageEnable = VK_FALSE;
		multisampling.alphaToOneEnable = VK_FALSE;

		return multisampling;
	}

	inline VkPipelineColorBlendAttachmentState PipelineColorBlendAttachmentState()
	{
		VkPipelineColorBlendAttachmentState colorBlendAttachment{};
		colorBlendAttachment.colorWriteMask =
		    VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
		colorBlendAttachment.blendEnable = VK_TRUE;
		colorBlendAttachment.srcColorBlendFactor = VK_BLEND_FACTOR_SRC_ALPHA;
		colorBlendAttachment.dstColorBlendFactor = VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
		colorBlendAttachment.colorBlendOp = VK_BLEND_OP_ADD;
		colorBlendAttachment.srcAlphaBlendFactor = VK_BLEND_FACTOR_ONE;
		colorBlendAttachment.dstAlphaBlendFactor = VK_BLEND_FACTOR_ZERO;
		colorBlendAttachment.alphaBlendOp = VK_BLEND_OP_ADD;

		return colorBlendAttachment;
	}

	inline VkPipelineLayoutCreateInfo PipelineLayoutCreateInfo()
	{
		VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
		pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
		pipelineLayoutInfo.pNext = nullptr;

		pipelineLayoutInfo.flags = 0;
		pipelineLayoutInfo.setLayoutCount = 0;
		pipelineLayoutInfo.pSetLayouts = nullptr;
		pipelineLayoutInfo.pushConstantRangeCount = 0;
		pipelineLayoutInfo.pPushConstantRanges = nullptr;

		return pipelineLayoutInfo;
	}

	inline VkRenderingAttachmentInfo RenderingAttachmentInfo( VkImageView imageView, VkImageLayout imageLayout )
	{
		VkRenderingAttachmentInfo renderingAttachmentInfo = {};
		renderingAttachmentInfo.sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO;
		renderingAttachmentInfo.pNext = nullptr;

		renderingAttachmentInfo.imageLayout = imageLayout;
		renderingAttachmentInfo.resolveMode = VK_RESOLVE_MODE_NONE;
		renderingAttachmentInfo.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
		renderingAttachmentInfo.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
		renderingAttachmentInfo.imageView = imageView;

		return renderingAttachmentInfo;
	}

	inline VkRenderingInfo RenderingInfo(
	    VkRenderingAttachmentInfo* colorAttachmentInfo, VkRenderingAttachmentInfo* depthAttachmentInfo, Size2D size )
	{
		VkExtent2D extent = { size.x, size.y };

		VkRenderingInfo renderInfo = {};
		renderInfo.sType = VK_STRUCTURE_TYPE_RENDERING_INFO;
		renderInfo.pNext = nullptr;
		renderInfo.layerCount = 1;
		renderInfo.renderArea = VkRect2D{ VkOffset2D{}, extent };
		renderInfo.colorAttachmentCount = 1;
		renderInfo.pColorAttachments = colorAttachmentInfo;
		renderInfo.pDepthAttachment = depthAttachmentInfo;
		renderInfo.pStencilAttachment = depthAttachmentInfo;

		return renderInfo;
	}

	inline VkSubmitInfo SubmitInfo( VkCommandBuffer* commandBuffer )
	{
		VkSubmitInfo submit = {};
		submit.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		submit.pNext = nullptr;

		submit.pWaitDstStageMask = nullptr;

		submit.waitSemaphoreCount = 0;
		submit.pWaitSemaphores = nullptr;

		submit.signalSemaphoreCount = 0;
		submit.pSignalSemaphores = nullptr;

		submit.commandBufferCount = 1;
		submit.pCommandBuffers = commandBuffer;

		return submit;
	}

	inline VkPresentInfoKHR PresentInfo( VkSwapchainKHR* swapchain, VkSemaphore* waitSemaphore, uint32_t* swapchainImageIndex )
	{
		VkPresentInfoKHR presentInfo = {};
		presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
		presentInfo.pNext = nullptr;

		presentInfo.pSwapchains = swapchain;
		presentInfo.swapchainCount = 1;

		presentInfo.pWaitSemaphores = waitSemaphore;
		presentInfo.waitSemaphoreCount = 1;

		presentInfo.pImageIndices = swapchainImageIndex;

		return presentInfo;
	}

	inline VkCommandBufferBeginInfo CommandBufferBeginInfo( VkCommandBufferUsageFlags flags )
	{
		VkCommandBufferBeginInfo cmdBeginInfo = {};

		cmdBeginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		cmdBeginInfo.pNext = nullptr;
		cmdBeginInfo.pInheritanceInfo = nullptr;
		cmdBeginInfo.flags = flags;

		return cmdBeginInfo;
	}

	inline VkImageCreateInfo ImageCreateInfo( VkFormat format, VkImageUsageFlags usageFlags, VkExtent3D extent,
	    uint32_t mipLevels, VkSampleCountFlagBits sampleCount = VK_SAMPLE_COUNT_1_BIT )
	{
		VkImageCreateInfo imageInfo = {};
		imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
		imageInfo.pNext = nullptr;

		imageInfo.imageType = VK_IMAGE_TYPE_2D;

		imageInfo.format = format;
		imageInfo.extent = extent;

		imageInfo.mipLevels = mipLevels;
		imageInfo.arrayLayers = 1;
		imageInfo.samples = sampleCount;
		imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;

		imageInfo.usage = usageFlags;
		imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;

		return imageInfo;
	}

	inline VkImageViewCreateInfo ImageViewCreateInfo(
	    VkFormat format, VkImage image, VkImageAspectFlags aspectFlags, uint32_t mipLevels )
	{
		VkImageViewCreateInfo viewInfo = {};
		viewInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
		viewInfo.pNext = nullptr;

		viewInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
		viewInfo.image = image;
		viewInfo.format = format;

		viewInfo.subresourceRange.baseMipLevel = 0;
		viewInfo.subresourceRange.levelCount = mipLevels;
		viewInfo.subresourceRange.baseArrayLayer = 0;
		viewInfo.subresourceRange.layerCount = 1;

		viewInfo.subresourceRange.aspectMask = aspectFlags;

		return viewInfo;
	}

	inline VkPipelineDepthStencilStateCreateInfo DepthStencilCreateInfo(
	    bool bDepthTest, bool bDepthWrite, VkCompareOp compareOp )
	{
		VkPipelineDepthStencilStateCreateInfo info = {};
		info.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
		info.pNext = nullptr;

		info.depthTestEnable = bDepthTest ? VK_TRUE : VK_FALSE;
		info.depthWriteEnable = bDepthWrite ? VK_TRUE : VK_FALSE;
		info.depthCompareOp = bDepthTest ? compareOp : VK_COMPARE_OP_ALWAYS;
		info.depthBoundsTestEnable = VK_FALSE;
		info.minDepthBounds = 0.0f; // Optional
		info.maxDepthBounds = 1.0f; // Optional
		info.stencilTestEnable = VK_FALSE;

		return info;
	}

	inline VkImageMemoryBarrier ImageMemoryBarrier(
	    VkAccessFlags accessMask, VkImageLayout oldLayout, VkImageLayout newLayout, VkImage image )
	{
		VkImageMemoryBarrier imageMemoryBarrier = {};
		imageMemoryBarrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
		imageMemoryBarrier.srcAccessMask = accessMask;
		imageMemoryBarrier.oldLayout = oldLayout;
		imageMemoryBarrier.newLayout = newLayout;
		imageMemoryBarrier.image = image;

		imageMemoryBarrier.subresourceRange = {};
		imageMemoryBarrier.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		imageMemoryBarrier.subresourceRange.baseMipLevel = 0;
		imageMemoryBarrier.subresourceRange.levelCount = VK_REMAINING_MIP_LEVELS;
		imageMemoryBarrier.subresourceRange.baseArrayLayer = 0;
		imageMemoryBarrier.subresourceRange.layerCount = VK_REMAINING_ARRAY_LAYERS;

		return imageMemoryBarrier;
	}

	inline VkDescriptorSetLayoutCreateInfo DescriptorSetLayoutCreateInfo(
	    VkDescriptorSetLayoutBinding* bindings, uint32_t bindingCount )
	{
		VkDescriptorSetLayoutCreateInfo descriptorSetLayoutInfo = {};
		descriptorSetLayoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		descriptorSetLayoutInfo.pNext = nullptr;

		descriptorSetLayoutInfo.bindingCount = bindingCount;
		descriptorSetLayoutInfo.pBindings = bindings;

		return descriptorSetLayoutInfo;
	}

	inline VkDescriptorSetAllocateInfo DescriptorSetAllocateInfo(
	    VkDescriptorPool descriptorPool, VkDescriptorSetLayout* descriptorSetLayout, uint32_t count )
	{
		VkDescriptorSetAllocateInfo descriptorSetAllocateInfo = {};
		descriptorSetAllocateInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
		descriptorSetAllocateInfo.pNext = nullptr;

		descriptorSetAllocateInfo.descriptorPool = descriptorPool;
		descriptorSetAllocateInfo.descriptorSetCount = count;
		descriptorSetAllocateInfo.pSetLayouts = descriptorSetLayout;

		return descriptorSetAllocateInfo;
	}

	inline VkSamplerCreateInfo SamplerCreateInfo(
	    VkFilter filters, VkSamplerAddressMode samplerAddressMode, bool anisoEnabled = false )
	{
		VkSamplerCreateInfo info = {};
		info.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
		info.pNext = nullptr;

		info.magFilter = filters;
		info.minFilter = filters;
		info.addressModeU = samplerAddressMode;
		info.addressModeV = samplerAddressMode;
		info.addressModeW = samplerAddressMode;

		info.anisotropyEnable = anisoEnabled;
		info.maxAnisotropy = anisoEnabled ? 16.0f : 0.0f;

		info.mipmapMode = VK_SAMPLER_MIPMAP_MODE_NEAREST;
		info.minLod = 0.0f;
		info.maxLod = 5.0f;

		return info;
	}

	inline VkWriteDescriptorSet WriteDescriptorImage(
	    VkDescriptorType type, VkDescriptorSet dstSet, VkDescriptorImageInfo* imageInfo, uint32_t binding )
	{
		VkWriteDescriptorSet write = {};
		write.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
		write.pNext = nullptr;

		write.dstBinding = binding;
		write.dstSet = dstSet;
		write.descriptorCount = 1;
		write.descriptorType = type;
		write.pImageInfo = imageInfo;

		return write;
	}
} // namespace VKInit