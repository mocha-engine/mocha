#pragma once

#include "game/g_model.h"
#include "globalvars.h"
#include <spdlog/spdlog.h>
#include "vulkan/vk_engine.h"
#include "vulkan/vk_mesh.h"

//@InteropGen generate class
class ManagedModel
{
private:
	Mesh mesh;

public:
	inline void SetIndexData( int size, void* data )
	{
		spdlog::info( "ManagedModel: Received {} index bytes", size );

		// Convert data to indices
		uint32_t* indices = ( uint32_t* )data;
		size_t indexCount = size / sizeof( uint32_t );

		mesh.indices.resize( indexCount );
		mesh.indices.insert( mesh.indices.begin(), indices, indices + indexCount );
	}

	inline void SetVertexData( int size, void* data )
	{
		spdlog::info( "ManagedModel: Received {} vertex bytes", size );
		
		// Convert data to vertices
		Vertex* vertices = ( Vertex* )data;
		size_t vertCount = size / sizeof( Vertex );

		mesh.vertices.resize( vertCount );
		mesh.vertices.insert( mesh.vertices.begin(), vertices, vertices + vertCount );
	}

	inline void Finish()
	{
		spdlog::info( "ManagedModel: built model" );

		Model model;
		model.InitPipelines();
		model.UploadMesh( mesh );

		Global::g_engine->m_triangle = model;
	}
};