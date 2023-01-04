#pragma once

#include <fstream>
#include <glm/ext.hpp>
#include <glm/glm.hpp>
#include <material.h>
#include <mathtypes.h>
#include <mesh.h>
#include <rendering.h>
#include <shadercompiler.h>
#include <spdlog/spdlog.h>
#include <texture.h>
#include <vector>

//@InteropGen generate class
class Model
{
private:
	void UploadMesh( Mesh& mesh );

public:
	std::vector<Mesh> m_meshes;
	bool m_hasIndexBuffer;
	bool m_isInitialized;

	void AddMesh( UtilArray vertices, UtilArray indices, Material* material );

	//@InteropGen ignore
	const std::vector<Mesh> GetMeshes() { return m_meshes; }
};