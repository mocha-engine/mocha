#include "CShader.h"

#include "Assert.h"
#include "CEngine.h"
#include "CRenderer.h"
#include "Globals.h"

#include <spdlog/spdlog.h>

void CShader::CreatePipeline()
{
	CRenderer* renderer = g_Engine->GetRenderer();
	ID3D12Device* device = renderer->GetDevice();
	ID3D12RootSignature* rootSignature = renderer->GetRootSignature();

	Compile();

	// Define the vertex input layout.
	D3D12_INPUT_ELEMENT_DESC inputElementDescs[] = {
	    { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 },
	    { "COLOR", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0 } };

	// Create the UBO.
	{
		// TODO;
	}

	// Describe and create the graphics pipeline state object (PSO).
	D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
	psoDesc.InputLayout = { inputElementDescs, _countof( inputElementDescs ) };
	psoDesc.pRootSignature = rootSignature;

	D3D12_SHADER_BYTECODE vsBytecode;
	D3D12_SHADER_BYTECODE psBytecode;

	vsBytecode.pShaderBytecode = mVertexShader->GetBufferPointer();
	vsBytecode.BytecodeLength = mVertexShader->GetBufferSize();

	psBytecode.pShaderBytecode = mPixelShader->GetBufferPointer();
	psBytecode.BytecodeLength = mPixelShader->GetBufferSize();

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
	for ( UINT i = 0; i < D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT; ++i )
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
		ASSERT( device->CreateGraphicsPipelineState( &psoDesc, IID_PPV_ARGS( &mPipelineState ) ) );
	}
	catch ( std::exception e )
	{
		spdlog::error( "Failed to create graphics pipeline." );
	}

	if ( mVertexShader )
	{
		mVertexShader->Release();
		mVertexShader = nullptr;
	}

	if ( mPixelShader )
	{
		mPixelShader->Release();
		mPixelShader = nullptr;
	}
}

CShader::CShader( const char* path, const char* source )
{
	mPath = path;
	mSource = source;
}

CShader::~CShader()
{
	if ( mPipelineState )
	{
		mPipelineState->Release();
		mPipelineState = nullptr;
	}
}

bool CShader::Compile()
{
	ID3DBlob* errors = nullptr;

	try
	{
		D3D_SHADER_MACRO vertexMacros[] = { { "VERTEX_SHADER", "1" }, { nullptr, nullptr } };
		D3D_SHADER_MACRO pixelMacros[] = { { "PIXEL_SHADER", "1" }, { nullptr, nullptr } };

		ASSERT( D3DCompile( mSource.c_str(), mSource.size(), mPath.c_str(), vertexMacros, nullptr, "Vertex", "vs_5_0",
		    D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, 0, &mVertexShader, &errors ) );

		ASSERT( D3DCompile( mSource.c_str(), mSource.size(), mPath.c_str(), pixelMacros, nullptr, "Pixel", "ps_5_0",
		    D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION, 0, &mPixelShader, &errors ) );
	}
	catch ( std::exception e )
	{
		const char* errStr = ( const char* )errors->GetBufferPointer();
		spdlog::error( errStr );
		errors->Release();
		errors = nullptr;

		return false;
	}

	return true;
}

ID3D12PipelineState* CShader::GetPipelineState()
{
	return mPipelineState;
}
