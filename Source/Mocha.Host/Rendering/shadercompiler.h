#pragma once

#include <slang.h>
#include <slang-com-ptr.h>
#include <vector>

#include "baserendercontext.h"

using namespace slang;


enum ShaderReflectionType
{
	SHADER_REFLECTION_TYPE_UNKNOWN,

	SHADER_REFLECTION_TYPE_BUFFER,
	SHADER_REFLECTION_TYPE_TEXTURE,
	SHADER_REFLECTION_TYPE_SAMPLER
};

struct ShaderReflectionBinding
{
	int Set;
	int Binding;
	ShaderReflectionType Type;
	const char* Name;
};

struct ShaderReflectionInfo
{
	UtilArray Bindings;
};

struct ShaderCompilerResult
{
	UtilArray ShaderData;
	ShaderReflectionInfo ReflectionData;
};

class ShaderCompiler
{
private:
	Slang::ComPtr<IGlobalSession> m_globalSession;

	ShaderCompiler();
	~ShaderCompiler();

public:
	static ShaderCompiler& Instance()
	{
		static ShaderCompiler* instance = new ShaderCompiler();
		return *instance;
	}

	bool Compile( const ShaderType shaderType, const char* pShader, ShaderCompilerResult& outResult );

	inline ShaderCompilerResult CompileOffline( const ShaderType shaderType, const char* pshader )
	{
		ShaderCompilerResult outResult;
		Compile( shaderType, pshader, outResult );

		return outResult;
	}
};