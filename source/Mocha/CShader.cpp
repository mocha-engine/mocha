#include "CShader.h"

#include "CShaderCompiler.h"
#include "Mocha.h"

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
	ASSERT( CShaderCompiler::Instance().Compile( vk::ShaderStageFlagBits::eFragment, mSource.c_str(), fragmentBits ) );

	std::vector<unsigned int> vertexBits;
	ASSERT( CShaderCompiler::Instance().Compile( vk::ShaderStageFlagBits::eFragment, mSource.c_str(), vertexBits ) );

	spdlog::info( "Successfully compiled '{}'.", mPath );

	return true;
}
