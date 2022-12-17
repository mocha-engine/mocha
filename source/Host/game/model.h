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
#include <vector>

struct MeshPushConstants
{
	glm::vec4 data;

	glm::mat4 modelMatrix;

	glm::mat4 renderMatrix;

	glm::vec3 cameraPos;
	float time;

	glm::vec4 vLightInfoWS[4];
};

//@InteropGen generate class
class Model
{
private:
	std::vector<Mesh> m_meshes;
	void UploadMesh( Mesh& mesh );

	bool m_hasIndexBuffer;
	bool m_isInitialized;

public:
	void AddMesh( InteropStruct vertices, InteropStruct indices, Material* material );

	//@InteropGen ignore
	void Render( VkCommandBuffer cmd, glm::mat4x4 viewProj, Transform transform );
};