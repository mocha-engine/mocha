#pragma once
#include "Vector.h"

struct Transform
{
	Vector_t position;
	Vector_t rotation;
	Vector_t scale;

	Transform()
	{
		position = Vector_t( 0, 0, 0 );
		rotation = Vector_t( 0, 0, 0 );
		scale = Vector_t( 1, 1, 1 );
	}

	Transform( Vector_t position_ )
	{
		position = position_;
		rotation = Vector_t( 0, 0, 0 );
		scale = Vector_t( 1, 1, 1 );
	}

	Transform( Vector_t position_, Vector_t rotation_ )
	{
		position = position_;
		rotation = rotation_;
		scale = Vector_t( 1, 1, 1 );
	}

	Transform( Vector_t position_, Vector_t rotation_, Vector_t scale_ )
	{
		position = position_;
		rotation = rotation_;
		scale = scale_;
	}
};

typedef struct Transform Transform_t;
