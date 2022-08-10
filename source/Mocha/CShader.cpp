#include "CShader.h"

#include "Assert.h"

#include <spdlog/spdlog.h>

CShader::CShader( const char* path, const char* source )
{
	mPath = path;
	mSource = source;
}

int CShader::Compile()
{
	ID3DBlob* errors = nullptr;

	try
	{
		D3D_SHADER_MACRO vertexMacros[] = { { "VERTEX_SHADER", "" }, { nullptr, nullptr } };
		D3D_SHADER_MACRO fragmentMacros[] = { { "FRAGMENT_SHADER", "" }, { nullptr, nullptr } };

		ASSERT( D3DCompile( mSource.c_str(), mSource.size(), mPath.c_str(), vertexMacros, nullptr, "main", "vs_5_0",
		    D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, 0, &mVertexShader, &errors ) );

		ASSERT( D3DCompile( mSource.c_str(), mSource.size(), mPath.c_str(), fragmentMacros, nullptr, "main", "ps_5_0",
		    D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, 0, &mFragmentShader, &errors ) );
	}
	catch ( std::exception e )
	{
		const char* errStr = ( const char* )errors->GetBufferPointer();
		spdlog::error( errStr );
		errors->Release();
		errors = nullptr;

		return 1;
	}

	return 0;
}
