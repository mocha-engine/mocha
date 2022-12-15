#pragma once
#include "types.h"

#include <glm/glm.hpp>
#include <material.h>
#include <vector>

struct VertexInputDescription
{
	std::vector<VkVertexInputBindingDescription> bindings;
	std::vector<VkVertexInputAttributeDescription> attributes;

	VkPipelineVertexInputStateCreateFlags flags = 0;
};

//@InteropGen generate struct
struct Vertex
{
	glm::vec3 position;
	glm::vec3 normal;
	glm::vec3 color;
	glm::vec2 uv;
	glm::vec3 tangent;
	glm::vec3 bitangent;

	static VertexInputDescription GetVertexDescription();
};

struct Mesh
{
	std::vector<Vertex> vertices;
	std::vector<uint32_t> indices;
	AllocatedBuffer vertexBuffer;
	AllocatedBuffer indexBuffer;

	Material material;

	Mesh( Material _material )
	    : material( _material )
	{
	}
};
