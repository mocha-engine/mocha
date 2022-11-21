#include "model.h"

#include "../vulkan/types.h"

#include <globalvars.h>
#include <vulkan/rendermanager.h>

void Model::InitPipelines()
{
	VkDevice device = g_renderManager->m_device;
	VkExtent2D windowExtent = g_renderManager->m_windowExtent;
	VkFormat colorFormat = g_renderManager->m_swapchainImageFormat;
	VkFormat depthFormat = g_renderManager->m_depthFormat;

	VkShaderModule triangleFragShader;
	if ( LoadShaderModule( "content/shaders/triangle.mshdr", VK_SHADER_STAGE_FRAGMENT_BIT, &triangleFragShader ) )
	{
		spdlog::info( "Frag shader compiled successfully" );
	}

	VkShaderModule triangleVertexShader;
	if ( LoadShaderModule( "content/shaders/triangle.mshdr", VK_SHADER_STAGE_VERTEX_BIT, &triangleVertexShader ) )
	{
		spdlog::info( "Vert shader compiled successfully" );
	}

	VkPipelineLayoutCreateInfo pipeline_layout_info = VKInit::PipelineLayoutCreateInfo();
	VkPushConstantRange push_constant = {};

	push_constant.offset = 0;
	push_constant.size = sizeof( MeshPushConstants );
	push_constant.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

	pipeline_layout_info.pPushConstantRanges = &push_constant;
	pipeline_layout_info.pushConstantRangeCount = 1;

	VK_CHECK( vkCreatePipelineLayout( device, &pipeline_layout_info, nullptr, &m_pipelineLayout ) );

	m_pipeline = PipelineFactory::begin()
	                 .WithFragmentShader( triangleFragShader )
	                 .WithVertexShader( triangleVertexShader )
	                 .WithVertexDescription( Vertex::GetVertexDescription() )
	                 .WithLayout( m_pipelineLayout )
	                 .Build( device, colorFormat, depthFormat );
}

void Model::UploadTriangleMesh()
{
	m_mesh.vertices.resize( 3 );

	m_mesh.vertices[0].position = { 1.0f, 1.0f, 0.0f };
	m_mesh.vertices[1].position = { -1.0f, 1.0f, 0.0f };
	m_mesh.vertices[2].position = { 0.0f, -1.0f, 0.0f };

	m_mesh.vertices[0].color = { 1.0f, 0.0f, 0.0f };
	m_mesh.vertices[1].color = { 0.0f, 1.0f, 0.0f };
	m_mesh.vertices[2].color = { 0.0f, 0.0f, 1.0f };

	UploadMesh( m_mesh );
}

void Model::UploadMesh( Mesh& mesh )
{
	m_mesh = mesh;

	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = m_mesh.vertices.size() * sizeof( Vertex );

	VmaAllocationCreateInfo vmaallocInfo = {};
	vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;

	//
	// Vertex buffer
	//
	{
		bufferInfo.usage = VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;
		
		VK_CHECK( vmaCreateBuffer( *g_allocator, &bufferInfo, &vmaallocInfo, &m_mesh.vertexBuffer.buffer,
		    &m_mesh.vertexBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, m_mesh.vertexBuffer.allocation, &data );
		memcpy( data, m_mesh.vertices.data(), m_mesh.vertices.size() * sizeof( Vertex ) );
		vmaUnmapMemory( *g_allocator, m_mesh.vertexBuffer.allocation );
	}

	//
	// Index buffer (if we have any indices)
	//

	if ( m_mesh.indices.size() > 0 )
	{
		bufferInfo.usage = VK_BUFFER_USAGE_INDEX_BUFFER_BIT;
		
		VK_CHECK( vmaCreateBuffer( *g_allocator, &bufferInfo, &vmaallocInfo, &m_mesh.indexBuffer.buffer,
		    &m_mesh.indexBuffer.allocation, nullptr ) );

		void* data;
		vmaMapMemory( *g_allocator, m_mesh.indexBuffer.allocation, &data );
		memcpy( data, m_mesh.indices.data(), m_mesh.indices.size() * sizeof( uint32_t ) );
		vmaUnmapMemory( *g_allocator, m_mesh.indexBuffer.allocation );

		m_bHasIndexBuffer = true;
	}
}

bool Model::LoadShaderModule( const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule )
{
	VkDevice device = g_renderManager->m_device;

	std::string line, text;
	std::ifstream in( filePath );

	while ( std::getline( in, line ) )
	{
		text += line + "\n";
	}

	const char* buffer = text.c_str();

	std::vector<unsigned int> shaderBits;
	ShaderCompiler::Instance().Compile( shaderStage, buffer, shaderBits );

	//
	//
	//

	VkShaderModuleCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	createInfo.pNext = nullptr;

	createInfo.codeSize = shaderBits.size() * sizeof( uint32_t );
	createInfo.pCode = shaderBits.data();

	VkShaderModule shaderModule;

	if ( vkCreateShaderModule( device, &createInfo, nullptr, &shaderModule ) != VK_SUCCESS )
	{
		spdlog::error( "Could not compile shader {}", filePath );
		return false;
	}

	*outShaderModule = shaderModule;
	return true;
}

void Model::Render( Camera* camera, VkCommandBuffer cmd, int frameNumber )
{
	vkCmdBindPipeline( cmd, VK_PIPELINE_BIND_POINT_GRAPHICS, m_pipeline );
	VkDeviceSize offset = 0;
	vkCmdBindVertexBuffers( cmd, 0, 1, &m_mesh.vertexBuffer.buffer, &offset );

	glm::mat4 vpMatrix = camera->GetProjectionViewMatrix();
	glm::mat4 model = glm::mat4{ 1.0f };
	glm::mat4 meshMatrix = vpMatrix * model;

	MeshPushConstants constants;
	constants.renderMatrix = meshMatrix;

	vkCmdPushConstants( cmd, m_pipelineLayout, VK_SHADER_STAGE_VERTEX_BIT, 0, sizeof( MeshPushConstants ), &constants );

	if ( m_bHasIndexBuffer )
	{
		vkCmdBindIndexBuffer( cmd, m_mesh.indexBuffer.buffer, offset, VK_INDEX_TYPE_UINT32 );
		uint32_t indexCount = static_cast<uint32_t>( m_mesh.indices.size() );
		vkCmdDrawIndexed( cmd, indexCount, 1, 0, 0, 0 );	
	}
	else 
	{
		uint32_t vertCount = static_cast<uint32_t>( m_mesh.vertices.size() );
		vkCmdDraw( cmd, vertCount, 1, 0, 0 );
	}
}