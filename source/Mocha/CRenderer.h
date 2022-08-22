#pragma once
#define GLM_FORCE_SSE42 1
#define GLM_FORCE_DEFAULT_ALIGNED_GENTYPES 1
#define GLM_FORCE_LEFT_HANDED
#include <glm/glm.hpp>
using namespace glm;

#include "CImgui.h"
#include "Uint2.h"

#include <functional>

typedef void ( *render_callback_fn )( ID3D12GraphicsCommandList* );

#include <memory>

class CWindow;

class CRenderer
{
private:
	void InitAPI( CWindow* window );
	void DestroyAPI();
	void InitResources();
	void DestroyResources();
	void CreateCommands();
	void DestroyCommands();
	void InitFramebuffer();
	void DestroyFramebuffer();
	void InitShaderResources();
	void DestroyShaderResources();
	void SetupSwapchain( unsigned width, unsigned height );

	unsigned mWidth, mHeight;
	CWindow* mWindow;

public:
	CRenderer( CWindow* window );
	~CRenderer();

	void BeginFrame();
	void EndFrame();

	void Resize( Uint2 size );
};
