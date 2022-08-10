#include "CShader.h"

#include "Assert.h"

#include <spdlog/spdlog.h>

CShader::CShader( const char* fragmentBytes, const char* vertexBytes )
{
	mVertexBytes = vertexBytes;
	mFragmentBytes = fragmentBytes;
}

int CShader::Compile()
{
	ID3DBlob* errors = nullptr;

	try
	{
		ASSERT( D3DCompile( mVertexBytes.c_str(), mVertexBytes.size(), nullptr, nullptr, nullptr, "main", "vs_5_0",
		    D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, 0, &mVertexShader, &errors ) );

		ASSERT( D3DCompile( mFragmentBytes.c_str(), mFragmentBytes.size(), nullptr, nullptr, nullptr, "main", "ps_5_0",
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
