#pragma once
#define GLM_FORCE_SSE42 1
#define GLM_FORCE_DEFAULT_ALIGNED_GENTYPES 1
#define GLM_FORCE_LEFT_HANDED
#include <glm/glm.hpp>
using namespace glm;

#include "CImgui.h"
#include "Uint2.h"
#include "d3d12.h"

#include <D3Dcompiler.h>
#include <dxgi1_4.h>
#include <functional>

#pragma comment( lib, "d3d12.lib" )
#pragma comment( lib, "dxgi.lib" )
#pragma comment( lib, "dxguid.lib" )
#pragma comment( lib, "d3dcompiler.lib" )

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

	static const UINT backbufferCount = 2;

	unsigned mWidth, mHeight;
	CWindow* mWindow;

	// Initialization
	IDXGIFactory4* mFactory;
	IDXGIAdapter1* mAdapter;
#if defined( _DEBUG )
	ID3D12Debug1* mDebugController;
	ID3D12DebugDevice* mDebugDevice;
#endif
	ID3D12Device* mDevice;
	ID3D12CommandQueue* mCommandQueue;
	ID3D12CommandAllocator* mCommandAllocator;
	ID3D12GraphicsCommandList* mCommandList;

	// Current Frame
	UINT mCurrentBuffer;
	ID3D12DescriptorHeap* mRtvHeap;
	ID3D12DescriptorHeap* mSrvHeap;
	ID3D12Resource* mRenderTargets[backbufferCount];
	IDXGISwapChain3* mSwapchain;

	// Resources
	D3D12_VIEWPORT mViewport;
	D3D12_RECT mSurfaceSize;

	UINT mRtvDescriptorSize;
	ID3D12RootSignature* mRootSignature;
	ID3D12PipelineState* mPipelineState;

	// Sync
	UINT mFrameIndex;
	HANDLE mFenceEvent;
	ID3D12Fence* mFence;
	UINT64 mFenceValue;

public:
	CRenderer( CWindow* window );
	~CRenderer();

	void BeginFrame();
	void EndFrame();

	void Resize( Uint2 size );

	ID3D12Device* GetDevice();
	ID3D12DescriptorHeap* GetSRVHeap();
	ID3D12GraphicsCommandList* GetCommandList();
};
