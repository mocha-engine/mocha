#pragma once

#include <glm/glm.hpp>

struct Vector3
{
	float x;
	float y;
	float z;

	inline glm::vec3 ToGLM() { return glm::vec3( x, y, z ); }
};

struct Vector4
{
	float x;
	float y;
	float z;
	float w;

	inline glm::vec4 ToGLM() { return glm::vec4( x, y, z, w ); }
};

struct Transform
{
	Vector3 position;
	Vector4 rotation;
	Vector3 scale;
};
