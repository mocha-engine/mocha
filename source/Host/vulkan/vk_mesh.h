#pragma once
#include "vk_types.h"

#include <glm/glm.hpp>
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

	static VertexInputDescription GetVertexDescription();
};

struct Mesh
{
	std::vector<Vertex> vertices;
	std::vector<uint32_t> indices;
	AllocatedBuffer vertexBuffer;
	AllocatedBuffer indexBuffer;
};
