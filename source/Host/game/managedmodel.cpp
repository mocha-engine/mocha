#include "ManagedModel.h"

#include "../vulkan/rendermanager.h"
#include "globalvars.h"

#include <edict.h>
#include <managedmaterial.h>
#include <modelentity.h>
#include <spdlog/spdlog.h>
#include <texture.h>

void ManagedModel::AddMesh( InteropStruct vertices, InteropStruct indices, ManagedMaterial* material )
{
	assert( vertices.size > 0 ); // Vertex buffer is not optional

	Mesh mesh( material->GetMaterial() );

	// Vertex buffer
	spdlog::info( "ManagedModel: Received {} vertex bytes", vertices.size );

	mesh.verticesSize = vertices.size;
	mesh.vertexData = std::shared_ptr<void>( vertices.data );

	// Index buffer, optional
	if ( indices.size > 0 )
	{
		spdlog::info( "ManagedModel: Received {} index bytes", indices.size );

		mesh.indicesSize = indices.size;
		mesh.indexData = std::shared_ptr<void>( indices.data );
	}

	mesh.indexCount = indices.count;
	mesh.vertexCount = vertices.count;

	m_model.UploadMesh( mesh );
}

Model ManagedModel::GetModel()
{
	spdlog::info( "ManagedModel: finish model" );
	return m_model;
}
