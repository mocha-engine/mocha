#pragma once
#include <d3d12.h>
#include <d3dcompiler.h>
#include <string>

//@InteropGen generate class
class CShader
{
private:
	ID3DBlob* mVertexShader;
	ID3DBlob* mPixelShader;

	ID3D12GraphicsCommandList* mCommandList;
	ID3D12PipelineState* mPipelineState;

	std::string mSource;
	std::string mPath;

	//@InteropGen ignore
	void CreatePipeline();

public:
	CShader( const char* path, const char* source );

	//@InteropGen ignore
	~CShader();

	bool Compile();

	//@InteropGen ignore
	ID3D12PipelineState* GetPipelineState();
};
