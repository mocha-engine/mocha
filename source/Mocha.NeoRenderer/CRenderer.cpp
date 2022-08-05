#include <spdlog/spdlog.h>

#include <format>
#include <iostream>
#include <fstream>

#include "Assert.h"
#include "CRenderer.h"
#include "CNativeWindow.h"

CRenderer::CRenderer(CNativeWindow* window)
{
	spdlog::info("Renderer init");
	mWindow = window;

	// Initialization
	mFactory = nullptr;
	mAdapter = nullptr;
#if defined( _DEBUG )
	mDebugController = nullptr;
#endif
	mDevice = nullptr;
	mCommandQueue = nullptr;
	mCommandAllocator = nullptr;
	mCommandList = nullptr;
	mSwapchain = nullptr;

	mRootSignature = nullptr;
	mPipelineState = nullptr;

	// Current Frame
	mRtvHeap = nullptr;
	for (size_t i = 0; i < backbufferCount; ++i)
	{
		mRenderTargets[i] = nullptr;
	}
	// Sync
	mFence = nullptr;

	InitAPI(window);
	InitResources();
	SetupCommands();
}

CRenderer::~CRenderer()
{
	spdlog::info("Destroying renderer");
	if (mSwapchain != nullptr)
	{
		mSwapchain->SetFullscreenState(false, nullptr);
		mSwapchain->Release();
		mSwapchain = nullptr;
	}

	DestroyCommands();
	DestroyFramebuffer();
	DestroyResources();
	DestroyAPI();
}

void CRenderer::InitAPI(CNativeWindow* window)
{
	UINT dxgiFactoryFlags = 0;
#if defined( _DEBUG )
	spdlog::info("Init debug");

	ID3D12Debug* debugController;
	ASSERT(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController)));
	ASSERT(debugController->QueryInterface(IID_PPV_ARGS(&mDebugController)));
	mDebugController->EnableDebugLayer();
	mDebugController->SetEnableGPUBasedValidation(true);

	dxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;

	debugController->Release();
	debugController = nullptr;

#endif
	ASSERT(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&mFactory)));

	// Create Adapter
	spdlog::info("Creating adapter");
	for (UINT adapterIndex = 0; DXGI_ERROR_NOT_FOUND != mFactory->EnumAdapters1(adapterIndex, &mAdapter); ++adapterIndex)
	{
		DXGI_ADAPTER_DESC1 desc;
		mAdapter->GetDesc1(&desc);

		if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
		{
			// Don't select the Basic Render Driver adapter.
			continue;
		}

		// Check to see if the adapter supports Direct3D 12, but don't create
		// the actual device yet.
		if (SUCCEEDED(D3D12CreateDevice(mAdapter, D3D_FEATURE_LEVEL_12_0, _uuidof(ID3D12Device), nullptr)))
		{
			break;
		}

		// We won't use this adapter, so release it
		mAdapter->Release();
	}

	// Create Device
	ID3D12Device* pDev = nullptr;
	ASSERT(D3D12CreateDevice(mAdapter, D3D_FEATURE_LEVEL_12_0, IID_PPV_ARGS(&mDevice)));

	mDevice->SetName(L"Hello Triangle Device");

#if defined( _DEBUG )
	// Get debug device
	ASSERT(mDevice->QueryInterface(&mDebugDevice));
#endif

	// Create Command Queue
	spdlog::info("Creating command queue");
	D3D12_COMMAND_QUEUE_DESC queueDesc = {};
	queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
	queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

	ASSERT(mDevice->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&mCommandQueue)));

	// Create Command Allocator
	ASSERT(mDevice->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&mCommandAllocator)));

	// Sync
	ASSERT(mDevice->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&mFence)));

	// Create Swapchain
	Resize(window->GetWindowSize());
}

void CRenderer::DestroyAPI()
{
	if (mFence)
	{
		mFence->Release();
		mFence = nullptr;
	}

	if (mCommandAllocator)
	{
		ASSERT(mCommandAllocator->Reset());
		mCommandAllocator->Release();
		mCommandAllocator = nullptr;
	}

	if (mCommandQueue)
	{
		mCommandQueue->Release();
		mCommandQueue = nullptr;
	}

	if (mDevice)
	{
		mDevice->Release();
		mDevice = nullptr;
	}

	if (mAdapter)
	{
		mAdapter->Release();
		mAdapter = nullptr;
	}

	if (mFactory)
	{
		mFactory->Release();
		mFactory = nullptr;
	}

#if defined( _DEBUG )
	if (mDebugController)
	{
		mDebugController->Release();
		mDebugController = nullptr;
	}

	D3D12_RLDO_FLAGS flags = D3D12_RLDO_SUMMARY | D3D12_RLDO_DETAIL | D3D12_RLDO_IGNORE_INTERNAL;

	mDebugDevice->ReportLiveDeviceObjects(flags);

	if (mDebugDevice)
	{
		mDebugDevice->Release();
		mDebugDevice = nullptr;
	}
#endif
}

void CRenderer::InitFramebuffer()
{
	mCurrentBuffer = mSwapchain->GetCurrentBackBufferIndex();

	// Create descriptor heaps.
	{
		// Describe and create a render target view (RTV) descriptor heap.
		D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
		rtvHeapDesc.NumDescriptors = backbufferCount;
		rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
		rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
		ASSERT(mDevice->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&mRtvHeap)));

		mRtvDescriptorSize = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
	}

	// Create frame resources.
	{
		D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle(mRtvHeap->GetCPUDescriptorHandleForHeapStart());

		// Create a RTV for each frame.
		for (UINT n = 0; n < backbufferCount; n++)
		{
			ASSERT(mSwapchain->GetBuffer(n, IID_PPV_ARGS(&mRenderTargets[n])));
			mDevice->CreateRenderTargetView(mRenderTargets[n], nullptr, rtvHandle);
			rtvHandle.ptr += (1 * mRtvDescriptorSize);
		}
	}
}

void CRenderer::DestroyFramebuffer()
{
	spdlog::info("Destroying framebuffer");
	for (size_t i = 0; i < backbufferCount; ++i)
	{
		if (mRenderTargets[i])
		{
			mRenderTargets[i]->Release();
			mRenderTargets[i] = 0;
		}
	}
	if (mRtvHeap)
	{
		mRtvHeap->Release();
		mRtvHeap = nullptr;
	}
}

void CRenderer::InitResources()
{
	// Create the root signature.
	spdlog::info("Init resources");
	{
		D3D12_FEATURE_DATA_ROOT_SIGNATURE featureData = {};

		// This is the highest version the sample supports. If
		// CheckFeatureSupport succeeds, the HighestVersion returned will not be
		// greater than this.
		featureData.HighestVersion = D3D_ROOT_SIGNATURE_VERSION_1_1;

		if (FAILED(mDevice->CheckFeatureSupport(D3D12_FEATURE_ROOT_SIGNATURE, &featureData, sizeof(featureData))))
		{
			featureData.HighestVersion = D3D_ROOT_SIGNATURE_VERSION_1_0;
		}

		D3D12_DESCRIPTOR_RANGE1 ranges[1];
		ranges[0].BaseShaderRegister = 0;
		ranges[0].RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
		ranges[0].NumDescriptors = 1;
		ranges[0].RegisterSpace = 0;
		ranges[0].OffsetInDescriptorsFromTableStart = 0;
		ranges[0].Flags = D3D12_DESCRIPTOR_RANGE_FLAG_NONE;

		D3D12_ROOT_PARAMETER1 rootParameters[1];
		rootParameters[0].ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
		rootParameters[0].ShaderVisibility = D3D12_SHADER_VISIBILITY_VERTEX;

		rootParameters[0].DescriptorTable.NumDescriptorRanges = 1;
		rootParameters[0].DescriptorTable.pDescriptorRanges = ranges;

		D3D12_VERSIONED_ROOT_SIGNATURE_DESC rootSignatureDesc;
		rootSignatureDesc.Version = D3D_ROOT_SIGNATURE_VERSION_1_1;
		rootSignatureDesc.Desc_1_1.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;
		rootSignatureDesc.Desc_1_1.NumParameters = 1;
		rootSignatureDesc.Desc_1_1.pParameters = rootParameters;
		rootSignatureDesc.Desc_1_1.NumStaticSamplers = 0;
		rootSignatureDesc.Desc_1_1.pStaticSamplers = nullptr;

		ID3DBlob* signature;
		ID3DBlob* error;
		try
		{
			ASSERT(D3D12SerializeVersionedRootSignature(&rootSignatureDesc, &signature, &error));
			ASSERT(mDevice->CreateRootSignature(
				0, signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&mRootSignature)));
			mRootSignature->SetName(L"Hello Triangle Root Signature");
		}
		catch (std::exception e)
		{
			const char* errStr = (const char*)error->GetBufferPointer();
			std::cout << errStr;
			error->Release();
			error = nullptr;
		}

		if (signature)
		{
			signature->Release();
			signature = nullptr;
		}
	}

	// Create the pipeline state, which includes compiling and loading shaders.
	{
		ID3DBlob* vertexShader = nullptr;
		ID3DBlob* pixelShader = nullptr;
		ID3DBlob* errors = nullptr;

#if defined( _DEBUG )
		// Enable better shader debugging with the graphics debugging tools.
		UINT compileFlags = D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION;
#else
		UINT compileFlags = 0;
#endif

		std::string path = "";
		char pBuf[1024];

		_getcwd(pBuf, 1024);
		path = pBuf;
		path += "\\";
		std::wstring wpath = std::wstring(path.begin(), path.end());

		std::string vertCompiledPath = path, fragCompiledPath = path;
		vertCompiledPath += "..\\content\\triangle.vert.dxbc";
		fragCompiledPath += "..\\content\\triangle.frag.dxbc";

		std::wstring vertPath = wpath + L"..\\content\\triangle.vert.hlsl";
		std::wstring fragPath = wpath + L"..\\content\\triangle.frag.hlsl";

		try
		{
			ASSERT(D3DCompileFromFile(
				vertPath.c_str(), nullptr, nullptr, "main", "vs_5_0", compileFlags, 0, &vertexShader, &errors));
			ASSERT(D3DCompileFromFile(
				fragPath.c_str(), nullptr, nullptr, "main", "ps_5_0", compileFlags, 0, &pixelShader, &errors));
		}
		catch (std::exception e)
		{
			const char* errStr = (const char*)errors->GetBufferPointer();
			std::cout << errStr;
			errors->Release();
			errors = nullptr;
		}

		std::ofstream vsOut(vertCompiledPath, std::ios::out | std::ios::binary),
			fsOut(fragCompiledPath, std::ios::out | std::ios::binary);

		vsOut.write((const char*)vertexShader->GetBufferPointer(), vertexShader->GetBufferSize());
		fsOut.write((const char*)pixelShader->GetBufferPointer(), pixelShader->GetBufferSize());

		// Define the vertex input layout.
		D3D12_INPUT_ELEMENT_DESC inputElementDescs[] = {
			{ "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
			{ "COLOR", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 } };

		// Create the UBO.
		{
		}

		// Describe and create the graphics pipeline state object (PSO).
		D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
		psoDesc.InputLayout = { inputElementDescs, _countof(inputElementDescs) };
		psoDesc.pRootSignature = mRootSignature;

		D3D12_SHADER_BYTECODE vsBytecode;
		D3D12_SHADER_BYTECODE psBytecode;

		vsBytecode.pShaderBytecode = vertexShader->GetBufferPointer();
		vsBytecode.BytecodeLength = vertexShader->GetBufferSize();

		psBytecode.pShaderBytecode = pixelShader->GetBufferPointer();
		psBytecode.BytecodeLength = pixelShader->GetBufferSize();

		psoDesc.VS = vsBytecode;
		psoDesc.PS = psBytecode;

		D3D12_RASTERIZER_DESC rasterDesc = {};
		rasterDesc.FillMode = D3D12_FILL_MODE_SOLID;
		rasterDesc.CullMode = D3D12_CULL_MODE_NONE;
		rasterDesc.FrontCounterClockwise = FALSE;
		rasterDesc.DepthBias = D3D12_DEFAULT_DEPTH_BIAS;
		rasterDesc.DepthBiasClamp = D3D12_DEFAULT_DEPTH_BIAS_CLAMP;
		rasterDesc.SlopeScaledDepthBias = D3D12_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
		rasterDesc.DepthClipEnable = TRUE;
		rasterDesc.MultisampleEnable = FALSE;
		rasterDesc.AntialiasedLineEnable = FALSE;
		rasterDesc.ForcedSampleCount = 0;
		rasterDesc.ConservativeRaster = D3D12_CONSERVATIVE_RASTERIZATION_MODE_OFF;

		psoDesc.RasterizerState = rasterDesc;

		D3D12_BLEND_DESC blendDesc = {};
		blendDesc.AlphaToCoverageEnable = FALSE;
		blendDesc.IndependentBlendEnable = FALSE;
		const D3D12_RENDER_TARGET_BLEND_DESC defaultRenderTargetBlendDesc = {
			FALSE,
			FALSE,
			D3D12_BLEND_ONE,
			D3D12_BLEND_ZERO,
			D3D12_BLEND_OP_ADD,
			D3D12_BLEND_ONE,
			D3D12_BLEND_ZERO,
			D3D12_BLEND_OP_ADD,
			D3D12_LOGIC_OP_NOOP,
			D3D12_COLOR_WRITE_ENABLE_ALL,
		};
		for (UINT i = 0; i < D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT; ++i)
			blendDesc.RenderTarget[i] = defaultRenderTargetBlendDesc;

		psoDesc.BlendState = blendDesc;
		psoDesc.DepthStencilState.DepthEnable = FALSE;
		psoDesc.DepthStencilState.StencilEnable = FALSE;
		psoDesc.SampleMask = UINT_MAX;
		psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
		psoDesc.NumRenderTargets = 1;
		psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
		psoDesc.SampleDesc.Count = 1;
		try
		{
			ASSERT(mDevice->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&mPipelineState)));
		}
		catch (std::exception e)
		{
			std::cout << "Failed to create Graphics Pipeline!";
		}

		if (vertexShader)
		{
			vertexShader->Release();
			vertexShader = nullptr;
		}

		if (pixelShader)
		{
			pixelShader->Release();
			pixelShader = nullptr;
		}
	}

	CreateCommands();

	// Command lists are created in the recording state, but there is nothing
	// to record yet. The main loop expects it to be closed, so close it now.
	ASSERT(mCommandList->Close());

	// Create synchronization objects and wait until assets have been uploaded
	// to the GPU.
	{
		mFenceValue = 1;

		// Create an event handle to use for frame synchronization.
		mFenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
		if (mFenceEvent == nullptr)
		{
			ASSERT(HRESULT_FROM_WIN32(GetLastError()));
		}

		// Wait for the command list to execute; we are reusing the same command
		// list in our main loop but for now, we just want to wait for setup to
		// complete before continuing.
		// Signal and increment the fence value.
		const UINT64 fence = mFenceValue;
		ASSERT(mCommandQueue->Signal(mFence, fence));
		mFenceValue++;

		// Wait until the previous frame is finished.
		if (mFence->GetCompletedValue() < fence)
		{
			ASSERT(mFence->SetEventOnCompletion(fence, mFenceEvent));
			WaitForSingleObject(mFenceEvent, INFINITE);
		}

		mFrameIndex = mSwapchain->GetCurrentBackBufferIndex();
	}
}

void CRenderer::DestroyResources()
{
	// Sync
	CloseHandle(mFenceEvent);

	if (mPipelineState)
	{
		mPipelineState->Release();
		mPipelineState = nullptr;
	}

	if (mRootSignature)
	{
		mRootSignature->Release();
		mRootSignature = nullptr;
	}
}

void CRenderer::CreateCommands()
{
	// Create the command list.
	ASSERT(mDevice->CreateCommandList(
		0, D3D12_COMMAND_LIST_TYPE_DIRECT, mCommandAllocator, mPipelineState, IID_PPV_ARGS(&mCommandList)));
	mCommandList->SetName(L"Hello Triangle Command List");
}

void CRenderer::SetupCommands()
{
	// Command list allocators can only be reset when the associated
	// command lists have finished execution on the GPU; apps should use
	// fences to determine GPU execution progress.
	ASSERT(mCommandAllocator->Reset());

	// However, when ExecuteCommandList() is called on a particular command
	// list, that command list can then be reset at any time and must be before
	// re-recording.
	ASSERT(mCommandList->Reset(mCommandAllocator, mPipelineState));

	// Set necessary state.
	mCommandList->SetGraphicsRootSignature(mRootSignature);
	mCommandList->RSSetViewports(1, &mViewport);
	mCommandList->RSSetScissorRects(1, &mSurfaceSize);

	// Indicate that the back buffer will be used as a render target.
	D3D12_RESOURCE_BARRIER renderTargetBarrier;
	renderTargetBarrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	renderTargetBarrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
	renderTargetBarrier.Transition.pResource = mRenderTargets[mFrameIndex];
	renderTargetBarrier.Transition.StateBefore = D3D12_RESOURCE_STATE_PRESENT;
	renderTargetBarrier.Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;
	renderTargetBarrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;

	mCommandList->ResourceBarrier(1, &renderTargetBarrier);

	D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle(mRtvHeap->GetCPUDescriptorHandleForHeapStart());
	rtvHandle.ptr = rtvHandle.ptr + (mFrameIndex * mRtvDescriptorSize);
	mCommandList->OMSetRenderTargets(1, &rtvHandle, FALSE, nullptr);

	// Record commands.
	const float clearColor[] = { .2f, .2f, .2f, 1.0f };
	mCommandList->ClearRenderTargetView(rtvHandle, clearColor, 0, nullptr);

	// Indicate that the back buffer will now be used to present.
	D3D12_RESOURCE_BARRIER presentBarrier;
	presentBarrier.Type = D3D12_RESOURCE_BARRIER_TYPE_TRANSITION;
	presentBarrier.Flags = D3D12_RESOURCE_BARRIER_FLAG_NONE;
	presentBarrier.Transition.pResource = mRenderTargets[mFrameIndex];
	presentBarrier.Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
	presentBarrier.Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;
	presentBarrier.Transition.Subresource = D3D12_RESOURCE_BARRIER_ALL_SUBRESOURCES;

	mCommandList->ResourceBarrier(1, &presentBarrier);

	ASSERT(mCommandList->Close());
}

void CRenderer::DestroyCommands()
{
	if (mCommandList)
	{
		mCommandList->Reset(mCommandAllocator, mPipelineState);
		mCommandList->ClearState(mPipelineState);
		ASSERT(mCommandList->Close());
		ID3D12CommandList* ppCommandLists[] = { mCommandList };
		mCommandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

		// Wait for GPU to finish work
		const UINT64 fence = mFenceValue;
		ASSERT(mCommandQueue->Signal(mFence, fence));
		mFenceValue++;
		if (mFence->GetCompletedValue() < fence)
		{
			ASSERT(mFence->SetEventOnCompletion(fence, mFenceEvent));
			WaitForSingleObject(mFenceEvent, INFINITE);
		}

		mCommandList->Release();
		mCommandList = nullptr;
	}
}

void CRenderer::SetupSwapchain(unsigned width, unsigned height)
{
	spdlog::info("Setting up swapchain");
	mSurfaceSize.left = 0;
	mSurfaceSize.top = 0;
	mSurfaceSize.right = static_cast<LONG>(mWidth);
	mSurfaceSize.bottom = static_cast<LONG>(mHeight);

	mViewport.TopLeftX = 0.0f;
	mViewport.TopLeftY = 0.0f;
	mViewport.Width = static_cast<float>(mWidth);
	mViewport.Height = static_cast<float>(mHeight);
	mViewport.MinDepth = .1f;
	mViewport.MaxDepth = 1000.f;

	// Update Uniforms
	float zoom = 2.5f;

	if (mSwapchain != nullptr)
	{
		mSwapchain->ResizeBuffers(backbufferCount, mWidth, mHeight, DXGI_FORMAT_R8G8B8A8_UNORM, 0);
	}
	else
	{
		DXGI_SWAP_CHAIN_DESC1 swapchainDesc = {};
		swapchainDesc.BufferCount = backbufferCount;
		swapchainDesc.Width = width;
		swapchainDesc.Height = height;
		swapchainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
		swapchainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		swapchainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
		swapchainDesc.SampleDesc.Count = 1;

		IDXGISwapChain1* swapchain;
		ASSERT(mFactory->CreateSwapChainForHwnd(mCommandQueue, mWindow->GetWindowHandle(), &swapchainDesc, nullptr, nullptr, &swapchain));
		HRESULT swapchainSupport = swapchain->QueryInterface(__uuidof(IDXGISwapChain3), (void**)&swapchain);
		if (SUCCEEDED(swapchainSupport))
		{
			mSwapchain = (IDXGISwapChain3*)swapchain;
		}
	}
	mFrameIndex = mSwapchain->GetCurrentBackBufferIndex();
}

void CRenderer::Resize(Uint2 size)
{
	spdlog::info("Got resize event");
	mWidth = clamp(size.x, 1u, 0xffffu);
	mHeight = clamp(size.y, 1u, 0xffffu);

	// WAITING FOR THE FRAME TO COMPLETE BEFORE CONTINUING IS NOT BEST PRACTICE.
	// This is code implemented as such for simplicity. The
	// D3D12HelloFrameBuffering sample illustrates how to use fences for
	// efficient resource usage and to maximize GPU utilization.

	// Signal and increment the fence value.
	const UINT64 fence = mFenceValue;
	ASSERT(mCommandQueue->Signal(mFence, fence));
	mFenceValue++;

	// Wait until the previous frame is finished.
	if (mFence->GetCompletedValue() < fence)
	{
		ASSERT(mFence->SetEventOnCompletion(fence, mFenceEvent));
		WaitForSingleObjectEx(mFenceEvent, INFINITE, false);
	}

	DestroyFramebuffer();
	SetupSwapchain(size.x, size.y);
	InitFramebuffer();
}

void CRenderer::Render()
{
	// Update Uniforms
	{
		D3D12_RANGE readRange;
		readRange.Begin = 0;
		readRange.End = 0;

		// TODO:
		/* UniformBuffer uboVS = camera->GetUniformBuffer(); */
		/* ASSERT( mUniformBuffer->Map( 0, &readRange, reinterpret_cast<void**>( &mMappedUniformBuffer ) ) ); */
		/* memcpy( mMappedUniformBuffer, &uboVS, sizeof( uboVS ) ); */
		/* mUniformBuffer->Unmap( 0, &readRange ); */
	}

	// Record all the commands we need to render the scene into the command
	// list.
	SetupCommands();

	// Execute the command list.
	ID3D12CommandList* ppCommandLists[] = { mCommandList };
	mCommandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);
	mSwapchain->Present(1, 0);

	// WAITING FOR THE FRAME TO COMPLETE BEFORE CONTINUING IS NOT BEST PRACTICE.

	// Signal and increment the fence value.
	const UINT64 fence = mFenceValue;
	ASSERT(mCommandQueue->Signal(mFence, fence));
	mFenceValue++;

	// Wait until the previous frame is finished.
	if (mFence->GetCompletedValue() < fence)
	{
		ASSERT(mFence->SetEventOnCompletion(fence, mFenceEvent));
		WaitForSingleObject(mFenceEvent, INFINITE);
	}

	mFrameIndex = mSwapchain->GetCurrentBackBufferIndex();
}