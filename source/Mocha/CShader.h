#pragma once
#include "CDeviceBuffer.h"
#include "CMochaEngine.h"
#include "CRenderer.h"
#include "Globals.h"

#include <string>

//@InteropGen generate class
class CShader
{
private:
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
	VkShaderModule CreateModule( std::vector<unsigned int> bytes );

	VkShaderModule mFragmentModule;
	VkShaderModule mVertexModule;

	VkPipeline mPipeline;
};

//@InteropGen generate class
namespace Renderer
{
	inline void DrawModel( CShader* shader, int indexCount, CDeviceBuffer* vertexBuffer, CDeviceBuffer* indexBuffer )
	{
		// STUB
	}
} // namespace Renderer