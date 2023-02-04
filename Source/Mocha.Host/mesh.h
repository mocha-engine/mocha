#pragma once

#include <material.h>
#include <rendering.h>
#include <util.h>

struct Mesh
{
	UtilArray vertices{};
	UtilArray indices{};

	VertexBuffer vertexBuffer{};
	IndexBuffer indexBuffer{};

	Material* material;

	std::string name{};

	Mesh( Material* _material )
	    : material( _material )
	{
	}

	Mesh( std::string _name, UtilArray _vertices, UtilArray _indices, Material* _material )
	    : name( _name )
	    , material( _material )
	    , indices( _indices )
	    , vertices( _vertices )
	{
	}
};
