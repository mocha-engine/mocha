#include "CShader.h"

#include "CRenderer.h"
#include "CShaderCompiler.h"
#include "CPipelineBuilder.h"
#include "Mocha.h"

#include <vulkan/vulkan.h>
#include <vulkan/vulkan.hpp>

void CShader::CreatePipeline()
{
	auto pipelineBuilder = CPipelineBuilder();
	pipelineBuilder.BuildPipeline( *g_Device, g_Engine->GetRenderer()->GetMainRenderPass(), this );

	spdlog::info( "Created shader pipeline successfully" );
}

CShader::CShader( const char* path, const char* source )
{
	mPath = path;
	mSource = source;
}

CShader::~CShader() {}

bool CShader::Compile()
{
	spdlog::info( "Compiling '{}'...", mPath );

	std::vector<unsigned int> fragmentBits;
	std::string fragmentSource = "#version 450\n#define FRAGMENT\n" + mSource;
	assert( CShaderCompiler::Instance().Compile( vk::ShaderStageFlagBits::eFragment, fragmentSource.c_str(), fragmentBits ) );

	std::vector<unsigned int> vertexBits;
	std::string vertexSource = "#version 450\n#define VERTEX\n" + mSource;
	assert( CShaderCompiler::Instance().Compile( vk::ShaderStageFlagBits::eVertex, vertexSource.c_str(), vertexBits ) );

	assert( fragmentBits.size() > 0 );
	assert( vertexBits.size() > 0 );

	mFragmentModule = CreateModule( fragmentBits );
	mVertexModule = CreateModule( vertexBits );
	
	spdlog::info( "Successfully compiled '{}'.", mPath );

	CreatePipeline();

	return true;
}

VkShaderModule CShader::CreateModule( std::vector<unsigned int> bytes )
{
	VkShaderModuleCreateInfo info = {};
	info.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
	info.pNext = nullptr;

	info.pCode = bytes.data();
	info.codeSize = bytes.size() * sizeof( unsigned int );

	VkShaderModule module;

	VkDevice device = *( VkDevice* )g_Device;
	ASSERT( vkCreateShaderModule( device, &info, nullptr, &module ) );

	return module;
}
