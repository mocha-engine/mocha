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

	std::string mSource;
	std::string mPath;

public:
	CShader( const char* path, const char* source );

	int Compile();
};
