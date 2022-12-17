#pragma once

#include "camera.h"

#include <fstream>
#include <game_types.h>
#include <glm/ext.hpp>
#include <glm/glm.hpp>
#include <mesh.h>
#include <pipeline.h>
#include <shadercompiler.h>
#include <spdlog/spdlog.h>
#include <texture.h>
#include <vector>
#include <vk_types.h>
#include <vkinit.h>

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