#pragma once
#include "types.h"

#include <glm/glm.hpp>
#include <managedtypes.h>
#include <material.h>
#include <vector>

struct Mesh
{
	InteropStruct vertices;
	InteropStruct indices;

	AllocatedBuffer vertexBuffer;
	AllocatedBuffer indexBuffer;

	Material material;

	Mesh( Material _material )
	    : material( _material )
	{
	}

	Mesh( InteropStruct _vertices, InteropStruct _indices, Material _material )
	    : material( _material )
	    , indices( _indices )
	    , vertices( _vertices )
	{
	}
};
