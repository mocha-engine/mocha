#include "model.h"

#include <Misc/globalvars.h>
#include <Rendering/rendering.h>
#include <Rendering/rendermanager.h>
#include <Root/root.h>
#include <Util/util.h>

void Model::UploadMesh( Mesh& mesh )
{
	//
	// Vertex buffer
	//
	{
		BufferInfo_t vertexBufferInfo = {};
		vertexBufferInfo.name = mesh.name + " vertex buffer";
		vertexBufferInfo.size = mesh.vertices.size;
		vertexBufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		vertexBufferInfo.usage = BUFFER_USAGE_FLAG_VERTEX_BUFFER | BUFFER_USAGE_FLAG_TRANSFER_DST;
		VertexBuffer vertexBuffer( m_parent, vertexBufferInfo );

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
		indexBufferInfo.name = mesh.name + " index buffer";
		indexBufferInfo.size = mesh.indices.size;
		indexBufferInfo.type = BUFFER_TYPE_VERTEX_INDEX_DATA;
		indexBufferInfo.usage = BUFFER_USAGE_FLAG_INDEX_BUFFER | BUFFER_USAGE_FLAG_TRANSFER_DST;
		IndexBuffer indexBuffer( m_parent, indexBufferInfo );

		BufferUploadInfo_t indexUploadInfo = {};
		indexUploadInfo.data = mesh.indices;
		indexBuffer.Upload( indexUploadInfo );

		mesh.indexBuffer = indexBuffer;
	}

	m_meshes.push_back( mesh );
	m_isInitialized = true;
}

void Model::AddMesh( const char* name, UtilArray vertices, UtilArray indices, Material* material )
{
	if ( vertices.size == 0 )
		return;

	Mesh mesh( std::string( name ), vertices, indices, material );
	UploadMesh( mesh );
}