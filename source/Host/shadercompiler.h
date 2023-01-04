#pragma once

#include <glslang/SPIRV/GlslangToSpv.h>
#include <rendering.h>
#include <volk.h>

//
// This compiles from GLSL to SPIR-V.
// It's not Vulkan-specific, so doesn't belong in
// Platform/Vulkan/.
//
class ShaderCompiler
{
private:
	inline ShaderCompiler() { glslang::InitializeProcess(); }
	inline ~ShaderCompiler() { glslang::FinalizeProcess(); }

	void InitResources( TBuiltInResource& Resources );
	EShLanguage FindLanguage( const ShaderType shader_type );
	static std::string GetPreamble( EShLanguage language );

public:
	static ShaderCompiler& Instance()
	{
		static ShaderCompiler* instance = new ShaderCompiler();
		return *instance;
	}

	bool Compile( const ShaderType shader_type, const char* pshader, std::vector<uint32_t>& spirv );
};