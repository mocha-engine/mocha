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

public:
	CShader( const char* path, const char* source );

	//@InteropGen ignore
	~CShader();

	bool Compile();
};

//@InteropGen generate class
namespace Renderer
{
	inline void DrawModel( CShader* shader, int indexCount, CDeviceBuffer* vertexBuffer, CDeviceBuffer* indexBuffer )
	{
		// STUB
	}
} // namespace Renderer