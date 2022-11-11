#include "g_model.h"

#include "../vulkan/vk_types.h"

void Model::InitPipelines( VmaAllocator allocator, VkDevice device, VkExtent2D windowExtent, VkFormat swapchainImageFormat )
{
	VkShaderModule triangleFragShader;
	if ( LoadShaderModule( device, "content/shaders/triangle.frag", VK_SHADER_STAGE_FRAGMENT_BIT, &triangleFragShader ) )
	{
		spdlog::info( "Frag shader compiled successfully" );
	}

	VkShaderModule triangleVertexShader;
	if ( LoadShaderModule( device, "content/shaders/triangle.vert", VK_SHADER_STAGE_VERTEX_BIT, &triangleVertexShader ) )
	{
		spdlog::info( "Vert shader compiled successfully" );
	}

	VkPipelineLayoutCreateInfo pipeline_layout_info = vkinit::PipelineLayoutCreateInfo();
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
	                    .Build( device, swapchainImageFormat );

	LoadMeshes( allocator );
}

void Model::LoadMeshes( VmaAllocator allocator )
{
	m_mesh.vertices.resize( 3 );

	m_mesh.vertices[0].position = { 1.0f, 1.0f, 0.0f };
	m_mesh.vertices[1].position = { -1.0f, 1.0f, 0.0f };
	m_mesh.vertices[2].position = { 0.0f, -1.0f, 0.0f };

	m_mesh.vertices[0].color = { 1.0f, 0.0f, 0.0f };
	m_mesh.vertices[1].color = { 0.0f, 1.0f, 0.0f };
	m_mesh.vertices[2].color = { 0.0f, 0.0f, 1.0f };

	UploadMesh( allocator, m_mesh );
}

void Model::UploadMesh( VmaAllocator allocator, Mesh& mesh )
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.pNext = nullptr;

	bufferInfo.size = mesh.vertices.size() * sizeof( Vertex );
	bufferInfo.usage = VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;

	VmaAllocationCreateInfo vmaallocInfo = {};
	vmaallocInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;

	VK_CHECK( vmaCreateBuffer(
	    allocator, &bufferInfo, &vmaallocInfo, &mesh.vertexBuffer.buffer, &mesh.vertexBuffer.allocation, nullptr ) );

	void* data;
	vmaMapMemory( allocator, mesh.vertexBuffer.allocation, &data );
	memcpy( data, mesh.vertices.data(), mesh.vertices.size() * sizeof( Vertex ) );
	vmaUnmapMemory( allocator, mesh.vertexBuffer.allocation );
}

bool Model::LoadShaderModule(
    VkDevice device, const char* filePath, VkShaderStageFlagBits shaderStage, VkShaderModule* outShaderModule )
{
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

	uint32_t vertCount = static_cast<uint32_t>( m_mesh.vertices.size() );
	vkCmdDraw( cmd, vertCount, 1, 0, 0 );
}