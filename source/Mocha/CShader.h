#pragma once
#include <d3d12.h>
#include <d3dcompiler.h>
#include <string>

//@InteropGen generate class
class CShader
{
private:
	ID3DBlob* mVertexShader;
	ID3DBlob* mFragmentShader;

	std::string mFragmentBytes;
	std::string mVertexBytes;

public:
	CShader( const char* fragmentBytes, const char* vertexBytes );

	int Compile();
};
