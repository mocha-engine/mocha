#pragma once
#include "types.h"

#include <glm/glm.hpp>
#include <material.h>
#include <vector>

struct Mesh
{
	int verticesSize;
	std::shared_ptr<void> vertexData;

	int indicesSize;
	std::shared_ptr<void> indexData;

	int vertexCount;
	int indexCount;
	
	AllocatedBuffer vertexBuffer;
	AllocatedBuffer indexBuffer;

	Material material;

	Mesh( Material _material )
	    : material( _material )
	{
	}
};
