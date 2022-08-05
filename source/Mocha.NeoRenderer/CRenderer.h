#pragma once
#define GLM_FORCE_SSE42 1
#define GLM_FORCE_DEFAULT_ALIGNED_GENTYPES 1
#define GLM_FORCE_LEFT_HANDED
#include <glm/glm.hpp>
using namespace glm;

#include "Uint2.h"

#include "d3d12.h"
#include <D3Dcompiler.h>
#include <dxgi1_4.h>

#pragma comment(lib, "d3d12.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "dxguid.lib")
#pragma comment(lib, "d3dcompiler.lib")

#include <memory>

class CNativeWindow;

class CRenderer
{
private:
	void InitAPI(CNativeWindow* window);
	void DestroyAPI();
	void InitResources();
	void DestroyResources();
	void CreateCommands();
	void SetupCommands();
	void DestroyCommands();
	void InitFramebuffer();
	void DestroyFramebuffer();
	void SetupSwapchain(unsigned width, unsigned height);

	static const UINT backbufferCount = 2;

	unsigned mWidth, mHeight;
	CNativeWindow* mWindow;

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
	CRenderer(CNativeWindow* window);
	~CRenderer();

	void Render();
	void Resize(Uint2 size);
};

