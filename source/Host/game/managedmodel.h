#pragma once

#include "vulkan/mesh.h"
#include "game/model.h"

struct Mesh;
class RenderManager;

//@InteropGen generate class
class ManagedModel
{
private:
	Mesh m_mesh;
	Model m_model;

public:
	void SetIndexData( int size, void* data );
	void SetVertexData( int size, void* data );
	void Finish();

	//@InteropGen ignore
	Model GetModel();
};