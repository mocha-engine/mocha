#pragma once

#include <game/g_model.h>
#include <globalvars.h>
#include <spdlog/spdlog.h>
#include <vulkan/vk_engine.h>
#include <vulkan/vk_mesh.h>

//@InteropGen generate class
class ManagedModel
{
private:
public:
	ManagedModel( int size, void* data )
	{
		spdlog::info( "ManagedModel: Received {} bytes", size );

		Mesh mesh;

		// Convert data to vertices
		Vertex* vertices = ( Vertex* )data;
		size_t vertCount = size / sizeof( Vertex );

		mesh.vertices.resize( vertCount );
		mesh.vertices.insert( mesh.vertices.begin(), vertices, vertices + vertCount );

		static Model model;
		model.InitPipelines();
		model.UploadMesh( mesh );

		spdlog::info( "ManagedModel: built model with {} vertices", vertCount );

		Global::g_engine->m_triangle = model;
	}
};