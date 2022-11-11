#pragma once
#include "../vulkan/vk_initializers.h"
#include "../vulkan/vk_mesh.h"
#include "../vulkan/vk_pipeline.h"
#include "../vulkan/vk_shadercompiler.h"
#include "../vulkan/vk_types.h"
#include "g_camera.h"
#include "g_types.h"

#include <fstream>
#include <glm/ext.hpp>
#include <glm/glm.hpp>
#include <spdlog/spdlog.h>

struct MeshPushConstants
{
	glm::vec4 data;
	glm::mat4 renderMatrix;
};

class Model
{
private:
	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;
	Mesh m_mesh;

public:
	void InitPipelines( VmaAllocator allocator, VkDevice device, VkExtent2D windowExtent, VkFormat swapchainImageFormat );

	void LoadMeshes( VmaAllocator allocator );

	inline void UploadMesh( VmaAllocator allocator, Mesh& mesh );

	bool LoadShaderModule(
	    VkDevice device, const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

	void Render( Camera* camera, VkCommandBuffer cmd, int frameNumber );
};

//@InteropGen generate class
class ManagedModel
{
private:
public:
	ManagedModel( int size, void* data ) {}
};