#pragma once

#include <game_types.h>
#include <glm/glm.hpp>
#include <managedtypes.h>
#include <material.h>
#include <vector>

struct Mesh
{
	InteropArray vertices;
	InteropArray indices;

	AllocatedBuffer vertexBuffer;
	AllocatedBuffer indexBuffer;

	Material material;

	Mesh( Material _material )
	    : material( _material )
	{
	}

	Mesh( InteropArray _vertices, InteropArray _indices, Material _material )
	    : material( _material )
	    , indices( _indices )
	    , vertices( _vertices )
	{
	}
};
