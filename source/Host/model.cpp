#include "model.h"

#include <globalvars.h>
#include <managedtypes.h>
#include <vk_types.h>
#include <rendermanager.h>

void Model::UploadMesh( Mesh& mesh )
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = mesh.vertices.size;

	VmaAllocationCreateInfo vmaallocInfo = {};
	vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;

	//
	// Vertex buffer
	//
	{
		bufferInfo.usage = VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;

		VK_CHECK( vmaCreateBuffer(
		    *g_allocator, &bufferInfo, &vmaallocInfo, &mesh.vertexBuffer.buffer, &mesh.vertexBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, mesh.vertexBuffer.allocation, &data );
		memcpy( data, mesh.vertices.data, mesh.vertices.size );
		vmaUnmapMemory( *g_allocator, mesh.vertexBuffer.allocation );
	}

	//
	// Index buffer (optional)
	//
	if ( mesh.indices.size > 0 )
	{
		bufferInfo.size = mesh.indices.size;
		bufferInfo.usage = VK_BUFFER_USAGE_INDEX_BUFFER_BIT;

		VK_CHECK( vmaCreateBuffer(
		    *g_allocator, &bufferInfo, &vmaallocInfo, &mesh.indexBuffer.buffer, &mesh.indexBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, mesh.indexBuffer.allocation, &data );
		memcpy( data, mesh.indices.data, mesh.indices.size );
		vmaUnmapMemory( *g_allocator, mesh.indexBuffer.allocation );

		m_hasIndexBuffer = true;
	}

	m_meshes.push_back( mesh );

	m_isInitialized = true;
}

void Model::AddMesh( InteropArray vertices, InteropArray indices, Material* material )
{
	if ( vertices.size == 0 )
		return;

	Mesh mesh( vertices, indices, *material );
	UploadMesh( mesh );
}

void Model::Render( VkCommandBuffer cmd, glm::mat4x4 viewProj, Transform transform )
{
	if ( !m_isInitialized )
		return;

	for ( Mesh& mesh : m_meshes )
	{
		auto material = mesh.material;

		vkCmdBindPipeline( cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, material.m_pipeline );
		VkDeviceSize offset = 0;
		vkCmdBindVertexBuffers( cmd, 0, 1, &mesh.vertexBuffer.buffer, &offset );

		glm::mat4x4 model = glm::mat4{ 1.0f };
		model *= glm::translate( glm::mat4{ 1.0f }, transform.position.ToGLM() );
		model *= glm::mat4_cast( transform.rotation.ToGLM() );
		model *= glm::scale( glm::mat4{ 1.0f }, transform.scale.ToGLM() );

		glm::mat4x4 renderMatrix = viewProj * model;

		MeshPushConstants constants = {};
		constants.modelMatrix = model;
		constants.renderMatrix = renderMatrix;
		constants.cameraPos = g_cameraPos.ToGLM();
		constants.time = g_curTime;
		constants.data.x = ( int )g_debugView;

		std::vector<Vector3> lightPositions = {};
		lightPositions.push_back( { 0, 4, 4 } );
		lightPositions.push_back( { 4, 0, 4 } );
		lightPositions.push_back( { 0, -4, 4 } );
		lightPositions.push_back( { -4, 0, 4 } );

		std::vector<glm::vec4> packedLightInfo = {};
		for ( int i = 0; i < 4; ++i )
		{
			packedLightInfo.push_back( { lightPositions[i].x, lightPositions[i].y, lightPositions[i].z, 10.0f } );
		}

		constants.vLightInfoWS[0] = packedLightInfo[0];
		constants.vLightInfoWS[1] = packedLightInfo[1];
		constants.vLightInfoWS[2] = packedLightInfo[2];
		constants.vLightInfoWS[3] = packedLightInfo[3];

		vkCmdBindDescriptorSets(
		    cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, material.m_pipelineLayout, 0, 1, &mesh.material.m_textureSet, 0, nullptr );
		vkCmdPushConstants( cmd, material.m_pipelineLayout, VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT, 0,
		    sizeof( MeshPushConstants ), &constants );

		if ( m_hasIndexBuffer )
		{
			vkCmdBindIndexBuffer( cmd, mesh.indexBuffer.buffer, offset, VK_INDEX_TYPE_UINT32 );
			vkCmdDrawIndexed( cmd, mesh.indices.count, 1, 0, 0, 0 );
		}
		else
		{
			vkCmdDraw( cmd, mesh.vertices.count, 1, 0, 0 );
		}
	}
}