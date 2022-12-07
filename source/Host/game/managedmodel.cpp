#include "ManagedModel.h"

#include "../vulkan/rendermanager.h"
#include "globalvars.h"

#include <edict.h>
#include <modelentity.h>
#include <spdlog/spdlog.h>
#include <managedtexture.h>
#include <texture.h>

void ManagedModel::SetIndexData( int size, void* data )
{
	spdlog::info( "ManagedModel: Received {} index bytes", size );

	// Convert data to indices
	uint32_t* indices = ( uint32_t* )data;
	size_t indexCount = size / sizeof( uint32_t );

	m_mesh.indices.resize( indexCount );
	m_mesh.indices.insert( m_mesh.indices.begin(), indices, indices + indexCount );
}

void ManagedModel::SetVertexData( int size, void* data )
{
	spdlog::info( "ManagedModel: Received {} vertex bytes", size );

	// Convert data to vertices
	Vertex* vertices = ( Vertex* )data;
	size_t vertCount = size / sizeof( Vertex );

	m_mesh.vertices.resize( vertCount );
	m_mesh.vertices.insert( m_mesh.vertices.begin(), vertices, vertices + vertCount );
}

void ManagedModel::Finish( ManagedTexture* texture )
{
	spdlog::info( "ManagedModel: built model" );

	m_model.SetTexture( texture->GetTexture() );
	
	m_model.InitDescriptors();
	m_model.InitPipelines();
	m_model.InitTextures();
	m_model.UploadMesh( m_mesh );
}

Model ManagedModel::GetModel()
{
	return m_model;
}
