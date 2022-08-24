#include "CDeviceBuffer.h"

#include "Assert.h"
#include "CMochaEngine.h"
#include "CRenderer.h"
#include "Globals.h"

#include <spdlog/spdlog.h>

CDeviceBuffer::CDeviceBuffer() {}
CDeviceBuffer::~CDeviceBuffer() {}

void CDeviceBuffer::CreateIndexBuffer( int size )
{
	mSize = size;
	mBufferType = BufferType::INDEX_BUFFER;
}

void CDeviceBuffer::CreateVertexBuffer( int size, int stride )
{
	mSize = size;
	mStride = stride;
	mBufferType = BufferType::VERTEX_BUFFER;
}

void CDeviceBuffer::SetData( void* data ) {}
