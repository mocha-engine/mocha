#pragma once

#include "game/model.h"
#include "vulkan/mesh.h"

class ManagedTexture;

//@InteropGen generate class
class ManagedModel
{
private:
	Mesh m_mesh;
	Model m_model;

public:
	void SetIndexData( int size, void* data );
	void SetVertexData( int size, void* data );
	void Finish( ManagedTexture* texture );

	//@InteropGen ignore
	Model GetModel();
};