#pragma once

#include <Rendering/Assets/material.h>
#include <Rendering/rendering.h>
#include <Util/util.h>

struct Mesh
{
	UtilArray vertices{};
	UtilArray indices{};

	VertexBuffer vertexBuffer{};
	IndexBuffer indexBuffer{};

	Material* material;

	std::string name{};

	bool isIndexed;

	Mesh( Material* _material )
	    : material( _material )
	{
	}

	Mesh( std::string _name, UtilArray _vertices, UtilArray _indices, Material* _material )
	    : name( _name )
	    , material( _material )
	    , indices( _indices )
	    , isIndexed( true )
	    , vertices( _vertices )
	{
	}

	Mesh( std::string _name, UtilArray _vertices, Material* _material )
	    : name( _name )
	    , material( _material )
	    , isIndexed( false )
	    , vertices( _vertices )
	{
	}
};
