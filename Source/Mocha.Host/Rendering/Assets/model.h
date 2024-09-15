#pragma once

#include <Misc/mathtypes.h>
#include <Rendering/Assets/material.h>
#include <Rendering/Assets/mesh.h>
#include <Rendering/Assets/texture.h>
#include <Rendering/rendering.h>
#include <fstream>
#include <glm/ext.hpp>
#include <glm/glm.hpp>
#include <spdlog/spdlog.h>
#include <vector>

class Root;

class Model
{
private:
	void UploadMesh( Mesh& mesh );

public:
	std::vector<Mesh> m_meshes;
	bool m_hasIndexBuffer;
	bool m_isInitialized;

	GENERATE_BINDINGS Model() {}

	GENERATE_BINDINGS void AddMesh( const char* name, UtilArray vertices, UtilArray indices, Material* material );
	GENERATE_BINDINGS void AddMesh( const char* name, UtilArray vertices, Material* material );

	const std::vector<Mesh> GetMeshes() { return m_meshes; }
};