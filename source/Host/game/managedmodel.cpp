#include "ManagedModel.h"

#include "../vulkan/rendermanager.h"
#include "globalvars.h"

#include <edict.h>
#include <managedmaterial.h>
#include <modelentity.h>
#include <spdlog/spdlog.h>
#include <texture.h>

void ManagedModel::AddMesh( int vertexCount, int vertexSize, void* vertexData, int indexCount, int indexSize, void* indexData,
    ManagedMaterial* material )
{
	assert( vertexSize > 0 ); // Vertex buffer is not optional

	Mesh mesh( material->GetMaterial() );

	// Vertex buffer
	spdlog::info( "ManagedModel: Received {} vertex bytes", vertexSize );

	mesh.verticesSize = vertexSize;
	mesh.vertexData = std::shared_ptr<void>( vertexData );

	// Index buffer, optional
	if ( indexSize > 0 )
	{
		spdlog::info( "ManagedModel: Received {} index bytes", indexSize );

		mesh.indicesSize = indexSize;
		mesh.indexData = std::shared_ptr<void>( indexData );
	}

	mesh.indexCount = indexCount;
	mesh.vertexCount = vertexCount;

	m_model.UploadMesh( mesh );
}

Model ManagedModel::GetModel()
{
	spdlog::info( "ManagedModel: finish model" );
	return m_model;
}
