#include "ManagedModel.h"

#include "../vulkan/rendermanager.h"
#include "globalvars.h"

#include <edict.h>
#include <managedtexture.h>
#include <modelentity.h>
#include <spdlog/spdlog.h>
#include <texture.h>

void ManagedModel::AddMesh( int vertexSize, void* vertexData, int indexSize, void* indexData, ManagedTexture* diffuseTexture )
{
	assert( vertexSize > 0 ); // Vertex buffer is not optional

	Material material( diffuseTexture->GetTexture() );

	Mesh mesh = {};
	mesh.material = material;

	// Vertex buffer
	spdlog::info( "ManagedModel: Received {} vertex bytes", vertexSize );

	// Convert data to vertices
	Vertex* vertices = ( Vertex* )vertexData;
	size_t vertCount = vertexSize / sizeof( Vertex );

	mesh.vertices.resize( vertCount );
	mesh.vertices.insert( mesh.vertices.begin(), vertices, vertices + vertCount );

	// Index buffer, optional
	if ( indexSize > 0 )
	{
		spdlog::info( "ManagedModel: Received {} index bytes", indexSize );

		// Convert data to indices
		uint32_t* indices = ( uint32_t* )indexData;
		size_t indexCount = indexSize / sizeof( uint32_t );

		mesh.indices.resize( indexCount );
		mesh.indices.insert( mesh.indices.begin(), indices, indices + indexCount );
	}

	m_model.UploadMesh( mesh );
}

Model ManagedModel::GetModel()
{
	spdlog::info( "ManagedModel: finish model" );
	return m_model;
}
