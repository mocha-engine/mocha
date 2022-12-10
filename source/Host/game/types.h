#pragma once

#include <glm/glm.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>
#include <thirdparty/imgui/imgui.h>

struct Vector2
{
	float x;
	float y;

	inline glm::vec2 ToGLM() { return glm::vec2( x, y ); }
	inline ImVec2 ToImGUI() { return { x, y }; };
};

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
