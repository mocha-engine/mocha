#pragma once

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
	int mStride;
	int mSize;
	BufferType mBufferType;

public:
	CDeviceBuffer();
	~CDeviceBuffer();

	void CreateIndexBuffer( int size );
	void CreateVertexBuffer( int size, int stride );

	void SetData( void* data );
};
