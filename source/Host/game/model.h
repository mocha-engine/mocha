#pragma once
#include "../vulkan/vkinit.h"
#include "../vulkan/mesh.h"
#include "../vulkan/pipeline.h"
#include "../vulkan/shadercompiler.h"
#include "../vulkan/types.h"
#include "camera.h"
#include "types.h"

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

	bool m_bHasIndexBuffer;

public:
	void InitPipelines();
	void UploadTriangleMesh();
	void UploadMesh( Mesh& mesh );
	bool LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

	void Render( Camera* camera, VkCommandBuffer cmd );
};