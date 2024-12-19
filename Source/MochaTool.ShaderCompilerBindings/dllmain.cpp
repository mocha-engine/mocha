#include "pch.h"
#include "Util/utilarray.h"
#include "Rendering/baserendercontext.h"
#include "Rendering/shadercompiler.h"

extern "C" __declspec( dllexport ) UtilArray CompileShader( const ShaderType shaderType, const char* pshader )
{
	return ShaderCompiler::Instance().CompileOffline( shaderType, pshader );
}