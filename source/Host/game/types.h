#pragma once

#include <glm/glm.hpp>
#include <glm/gtx/quaternion.hpp>
#include <glm/gtc/quaternion.hpp>

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

struct Quaternion
{
	float x;
	float y;
	float z;
	float w;

	inline glm::quat ToGLM() { return glm::quat( w, x, y, z ); }
};

struct Transform
{
	Vector3 position;
	Quaternion rotation;
	Vector3 scale;
};
