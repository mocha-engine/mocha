#pragma once
#include <d3d12.h>

enum BufferType
{
	VERTEX_BUFFER,
	INDEX_BUFFER,
	CONSTANT_BUFFER
};

//@InteropGen generate class
class CDeviceBuffer
{
private:
	ID3D12Resource* mDeviceBuffer;
	void* mDeviceBufferView;

	int mStride;
	int mSize;
	BufferType mBufferType;

	void InternalCreateBuffer();

public:
	CDeviceBuffer();
	~CDeviceBuffer();

	void CreateIndexBuffer( int size );
	void CreateVertexBuffer( int size, int stride );

	void SetData( void* data );
};
