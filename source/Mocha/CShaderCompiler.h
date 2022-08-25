#pragma once
#include <glslang/SPIRV/GlslangToSpv.h>
#include <vulkan/vulkan.hpp>

class CShaderCompiler
{
private:
	inline CShaderCompiler() { glslang::InitializeProcess(); }
	inline ~CShaderCompiler() { glslang::FinalizeProcess(); }

	void InitResources( TBuiltInResource& Resources );
	EShLanguage FindLanguage( const vk::ShaderStageFlagBits shader_type );
	static std::string GetPreamble( EShLanguage language );

public:
	static CShaderCompiler& Instance()
	{
		static CShaderCompiler* instance = new CShaderCompiler();
		return *instance;
	}

	bool Compile( const vk::ShaderStageFlagBits shader_type, const char* pshader, std::vector<unsigned int>& spirv );
};
