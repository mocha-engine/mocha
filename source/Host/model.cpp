#include "model.h"

#include <globalvars.h>
#include <managedtypes.h>
#include <rendermanager.h>
#include <vk_types.h>

void Model::UploadMesh( Mesh& mesh )
{
	VkBufferCreateInfo stagingBufferInfo = {};
	stagingBufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	stagingBufferInfo.pNext = nullptr;

	stagingBufferInfo.size = mesh.vertices.size;
	stagingBufferInfo.usage = VK_BUFFER_USAGE_TRANSFER_SRC_BIT;

	VmaAllocationCreateInfo vmaallocInfo = {};

	//
	// Vertex buffer
	//
	{
		AllocatedBuffer stagingBuffer = {};

		vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_ONLY;

		VK_CHECK( vmaCreateBuffer(
		    *g_allocator, &stagingBufferInfo, &vmaallocInfo, &stagingBuffer.buffer, &stagingBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, stagingBuffer.allocation, &data );
		memcpy( data, mesh.vertices.data, mesh.vertices.size );
		vmaUnmapMemory( *g_allocator, stagingBuffer.allocation );

		VkBufferCreateInfo vertexBufferInfo = {};
		vertexBufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		vertexBufferInfo.pNext = nullptr;

		vertexBufferInfo.size = mesh.vertices.size;
		vertexBufferInfo.usage = VK_BUFFER_USAGE_VERTEX_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT |
		                         VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT |
		                         VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_BUILD_INPUT_READ_ONLY_BIT_KHR;

		vmaallocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;

		VK_CHECK( vmaCreateBuffer( *g_allocator, &vertexBufferInfo, &vmaallocInfo, &mesh.vertexBuffer.buffer,
		    &mesh.vertexBuffer.allocation, nullptr ) );

		g_renderManager->ImmediateSubmit( [=]( VkCommandBuffer cmd ) {
			VkBufferCopy copy = {};
			copy.dstOffset = 0;
			copy.srcOffset = 0;
			copy.size = mesh.vertices.size;

			vkCmdCopyBuffer( cmd, stagingBuffer.buffer, mesh.vertexBuffer.buffer, 1, &copy );
		} );
	}

	//
	// Index buffer (optional)
	//
	if ( mesh.indices.size > 0 )
	{
		stagingBufferInfo.size = mesh.indices.size;
		AllocatedBuffer stagingBuffer = {};

		vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_ONLY;

		VK_CHECK( vmaCreateBuffer(
		    *g_allocator, &stagingBufferInfo, &vmaallocInfo, &stagingBuffer.buffer, &stagingBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, stagingBuffer.allocation, &data );
		memcpy( data, mesh.indices.data, mesh.indices.size );
		vmaUnmapMemory( *g_allocator, stagingBuffer.allocation );

		VkBufferCreateInfo indexBufferInfo = {};
		indexBufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		indexBufferInfo.pNext = nullptr;

		indexBufferInfo.size = mesh.indices.size;
		indexBufferInfo.usage = VK_BUFFER_USAGE_INDEX_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT |
		                        VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT |
		                        VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_BUILD_INPUT_READ_ONLY_BIT_KHR;

		vmaallocInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;

		VK_CHECK( vmaCreateBuffer(
		    *g_allocator, &indexBufferInfo, &vmaallocInfo, &mesh.indexBuffer.buffer, &mesh.indexBuffer.allocation, nullptr ) );

		g_renderManager->ImmediateSubmit( [=]( VkCommandBuffer cmd ) {
			VkBufferCopy copy = {};
			copy.dstOffset = 0;
			copy.srcOffset = 0;
			copy.size = mesh.indices.size;

			vkCmdCopyBuffer( cmd, stagingBuffer.buffer, mesh.indexBuffer.buffer, 1, &copy );
		} );

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
		auto& material = mesh.material;

		// JIT pipeline creation
		if ( mesh.material.m_pipeline == nullptr )
		{
			spdlog::trace( "Model::Render: Creating pipeline JIT..." );
			
			mesh.material.CreateResources();
		}

		vkCmdBindPipeline( cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, material.m_pipeline );
		VkDeviceSize offset = 0;
		vkCmdBindVertexBuffers( cmd, 0, 1, &mesh.vertexBuffer.buffer, &offset );

		glm::mat4x4 model = transform.GetModelMatrix();
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
			packedLightInfo.push_back( { lightPositions[i].x, lightPositions[i].y, lightPositions[i].z, 50.0f } );
		}

		constants.vLightInfoWS[0] = packedLightInfo[0];
		constants.vLightInfoWS[1] = packedLightInfo[1];
		constants.vLightInfoWS[2] = packedLightInfo[2];
		constants.vLightInfoWS[3] = packedLightInfo[3];

#if RAYTRACING
		VkDescriptorSet sets[] = { mesh.material.m_textureSet, mesh.material.m_accelerationStructureSet };

		vkCmdBindDescriptorSets( cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, material.m_pipelineLayout, 0, 2, &sets[0], 0, nullptr );
#else
		vkCmdBindDescriptorSets(
		    cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, material.m_pipelineLayout, 0, 1, &mesh.material.m_textureSet, 0, nullptr );
#endif

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