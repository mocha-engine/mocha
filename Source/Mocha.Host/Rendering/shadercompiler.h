#pragma once

#include "baserendercontext.h"

#include <slang-com-ptr.h>
#include <slang.h>
#include <vector>

using namespace slang;

enum ShaderReflectionType
{
	SHADER_REFLECTION_TYPE_UNKNOWN,

	SHADER_REFLECTION_TYPE_BUFFER,
	SHADER_REFLECTION_TYPE_TEXTURE,
	SHADER_REFLECTION_TYPE_SAMPLER
};

enum ShaderReflectionAttributeType
{
	SHADER_REFLECTION_ATTRIBUTE_TYPE_UNKNOWN,

	// SrgbReadAttribute
	SHADER_REFLECTION_ATTRIBUTE_TYPE_SRGB_READ,

	// DefaultAttribute
	SHADER_REFLECTION_ATTRIBUTE_TYPE_DEFAULT
};

struct DefaultAttributeData
{
	float ValueR = 0.0f;
	float ValueG = 0.0f;
	float ValueB = 0.0f;
	float ValueA = 0.0f;
};

struct ShaderReflectionAttribute
{
	ShaderReflectionAttributeType Type = SHADER_REFLECTION_ATTRIBUTE_TYPE_UNKNOWN;
	void* Data = nullptr;
};

struct ShaderReflectionBinding
{
	int Set = -1;
	int Binding = -1;
	ShaderReflectionType Type = SHADER_REFLECTION_TYPE_UNKNOWN;
	const char* Name = nullptr;

	UtilArray Attributes;
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