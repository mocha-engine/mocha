#pragma once
#include <glslang/SPIRV/GlslangToSpv.h>
#include <vulkan/vulkan.h>

class ShaderCompiler
{
private:
	inline ShaderCompiler() { glslang::InitializeProcess(); }
	inline ~ShaderCompiler() { glslang::FinalizeProcess(); }

	void InitResources( TBuiltInResource& Resources );
	EShLanguage FindLanguage( const VkShaderStageFlagBits shader_type );
	static std::string GetPreamble( EShLanguage language );

public:
	static ShaderCompiler& Instance()
	{
		static ShaderCompiler* instance = new ShaderCompiler();
		return *instance;
	}

	bool Compile( const VkShaderStageFlagBits shader_type, const char* pshader, std::vector<uint32_t>& spirv );
};