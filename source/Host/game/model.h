#pragma once
#include "../vulkan/mesh.h"
#include "../vulkan/pipeline.h"
#include "../vulkan/shadercompiler.h"
#include "../vulkan/types.h"
#include "../vulkan/vkinit.h"
#include "camera.h"
#include "types.h"

#include <fstream>
#include <glm/ext.hpp>
#include <glm/glm.hpp>
#include <spdlog/spdlog.h>
#include <texture.h>

struct MeshPushConstants
{
	glm::vec4 data;
	glm::mat4 modelMatrix;
	glm::mat4 renderMatrix;
	glm::vec3 cameraPos;
};

class Model
{
private:
	VkDescriptorSet m_textureSet;
	VkDescriptorSetLayout m_textureSetLayout;

	VkPipelineLayout m_pipelineLayout;
	VkPipeline m_pipeline;

	Texture m_texture;
	VkSampler m_textureSampler;

	Mesh m_mesh;

	bool m_hasIndexBuffer;

	bool m_isInitialized;

public:
	void InitDescriptors();
	void InitPipelines();
	void InitTextures();

	inline void SetTexture( Texture texture ) { m_texture = texture; }
	
	void UploadTriangleMesh();
	void UploadMesh( Mesh& mesh );
	bool LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule );

	void Render( VkCommandBuffer cmd, glm::mat4x4 viewProj, Transform transform );
};