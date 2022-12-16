#pragma once
#include <vector>
#include <vulkan/mesh.h>
#include <vulkan/vulkan.h>

enum VertexAttributeFormat
{
	Int,
	Float,
	Float2,
	Float3,
	Float4
};

struct VertexAttribute
{
	const char* name;
	int format;
};
