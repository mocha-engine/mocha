#pragma once

#include <slang.h>
#include <slang-com-ptr.h>
#include <Rendering/rendering.h>
#include <volk.h>

using namespace slang;

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

	bool Compile( const ShaderType shaderType, const char* pshader, std::vector<uint32_t>& outSpirv );
};