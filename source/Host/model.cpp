#include "model.h"

#include <globalvars.h>
#include <rendering.h>
#include <rendermanager.h>
#include <util.h>

void Model::UploadMesh( Mesh& mesh )
{
	//
	// Vertex buffer
	//
	{
		BufferInfo_t vertexBufferInfo = {};
		vertexBufferInfo.size = mesh.vertices.size;
		vertexBufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		VertexBuffer vertexBuffer( vertexBufferInfo );

		BufferUploadInfo_t vertexUploadInfo = {};
		vertexUploadInfo.data = mesh.vertices;
		vertexBuffer.Upload( vertexUploadInfo );

		mesh.vertexBuffer = vertexBuffer;
	}

	//
	// Index buffer (optional)
	//
	if ( mesh.indices.size > 0 )
	{
		BufferInfo_t indexBufferInfo = {};
		indexBufferInfo.size = mesh.indices.size;
		indexBufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		IndexBuffer indexBuffer( indexBufferInfo );

		BufferUploadInfo_t indexUploadInfo = {};
		indexUploadInfo.data = mesh.indices;
		indexBuffer.Upload( indexUploadInfo );

		mesh.indexBuffer = indexBuffer;
	}

	m_meshes.push_back( mesh );
	m_isInitialized = true;
}

void Model::AddMesh( UtilArray vertices, UtilArray indices, Material* material )
{
	if ( vertices.size == 0 )
		return;

	Mesh mesh( vertices, indices, *material );
	UploadMesh( mesh );
}