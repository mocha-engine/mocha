#pragma once

struct Vector
{
	float x;
	float y;
	float z;

	Vector()
	{
		x = 0;
		y = 0;
		z = 0;
	}

	Vector( float x_, float y_, float z_ )
	{
		x = x_;
		y = y_;
		z = z_;
	}
};

typedef struct Vector Vector_t;
