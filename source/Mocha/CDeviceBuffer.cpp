#include "CDeviceBuffer.h"

#include "Assert.h"
#include "CEngine.h"
#include "CRenderer.h"
#include "Globals.h"

#include <spdlog/spdlog.h>

CDeviceBuffer::CDeviceBuffer() {}
CDeviceBuffer::~CDeviceBuffer() {}

void CDeviceBuffer::InternalCreateBuffer()
{
	D3D12_HEAP_PROPERTIES heapProps;
	heapProps.Type = D3D12_HEAP_TYPE_UPLOAD;
	heapProps.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
	heapProps.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
	heapProps.CreationNodeMask = 1;
	heapProps.VisibleNodeMask = 1;

	D3D12_RESOURCE_DESC vertexBufferResourceDesc;
	vertexBufferResourceDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
	vertexBufferResourceDesc.Alignment = 0;
	vertexBufferResourceDesc.Width = mSize;
	vertexBufferResourceDesc.Height = 1;
	vertexBufferResourceDesc.DepthOrArraySize = 1;
	vertexBufferResourceDesc.MipLevels = 1;
	vertexBufferResourceDesc.Format = DXGI_FORMAT_UNKNOWN;
	vertexBufferResourceDesc.SampleDesc.Count = 1;
	vertexBufferResourceDesc.SampleDesc.Quality = 0;
	vertexBufferResourceDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
	vertexBufferResourceDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

	auto device = g_Engine->GetRenderer()->GetDevice();

	ASSERT( device->CreateCommittedResource( &heapProps, D3D12_HEAP_FLAG_NONE, &vertexBufferResourceDesc,
	    D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS( &mDeviceBuffer ) ) );
}

void CDeviceBuffer::CreateIndexBuffer( int size )
{
	mSize = size;
	mBufferType = BufferType::INDEX_BUFFER;

	InternalCreateBuffer();
}

void CDeviceBuffer::CreateVertexBuffer( int size, int stride )
{
	mSize = size;
	mStride = stride;
	mBufferType = BufferType::VERTEX_BUFFER;

	InternalCreateBuffer();
}

void CDeviceBuffer::SetData( void* data )
{
	void* mappedDataBegin;

	// These ranges only affect reading from the CPU
	D3D12_RANGE readRange;
	readRange.Begin = 0;
	readRange.End = 0;

	ASSERT( mDeviceBuffer->Map( 0, &readRange, reinterpret_cast<void**>( &mappedDataBegin ) ) );
	memcpy( mappedDataBegin, data, sizeof( data ) );
	mDeviceBuffer->Unmap( 0, nullptr );

	if ( mBufferType == BufferType::INDEX_BUFFER )
	{
		auto indexBufferView = D3D12_INDEX_BUFFER_VIEW{};
		indexBufferView.BufferLocation = mDeviceBuffer->GetGPUVirtualAddress();
		indexBufferView.SizeInBytes = mSize;
		indexBufferView.Format = DXGI_FORMAT_R32_UINT;

		mDeviceBufferView = reinterpret_cast<void*>( &indexBufferView );

		spdlog::info( "Setting index buffer data" );

		// Debug: show indices in console
		unsigned int* start = static_cast<unsigned int*>( data );

		for ( int i = 0; i < mSize / sizeof( unsigned int ); i++ )
		{
			unsigned int index = start[i];
			spdlog::info( "[C++] Index {}: {}", i, index );
		}

		spdlog::trace( "[C++] Pointer: {}", data );
	}
	else if ( mBufferType == BufferType::VERTEX_BUFFER )
	{
		auto vertexBufferView = D3D12_VERTEX_BUFFER_VIEW{};
		vertexBufferView.BufferLocation = mDeviceBuffer->GetGPUVirtualAddress();
		vertexBufferView.SizeInBytes = mSize;
		vertexBufferView.StrideInBytes = mStride;

		mDeviceBufferView = reinterpret_cast<void*>( &vertexBufferView );

		spdlog::info( "Setting vertex buffer data" );

		// Debug: show vertices in console
		float* start = static_cast<float*>( data );

		for ( int i = 0; i < mSize / sizeof( float ); i++ )
		{
			float v = start[i];
			spdlog::info( "[C++] Vertex {}: {}", i, v );
		}
	}
}