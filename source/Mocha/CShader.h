#pragma once
#include "CDeviceBuffer.h"
#include "CEngine.h"
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
	// TODO: We'll do SceneObjects and such later.. this is temp
	inline void DrawModel( CShader* shader, int indexCount, CDeviceBuffer* vertexBuffer, CDeviceBuffer* indexBuffer )
	{
		// TODO
	}
} // namespace Renderer