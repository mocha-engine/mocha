#include "mesh.h"

VertexInputDescription Vertex::GetVertexDescription()
{
	VertexInputDescription description;

	VkVertexInputBindingDescription mainBinding = {};
	mainBinding.binding = 0;
	mainBinding.stride = sizeof( Vertex );
	mainBinding.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

	description.bindings.push_back( mainBinding );

	VkVertexInputAttributeDescription positionAttribute = {};
	positionAttribute.binding = 0;
	positionAttribute.location = 0;
	positionAttribute.format = VK_FORMAT_R32G32B32_SFLOAT;
	positionAttribute.offset = offsetof( Vertex, position );

	VkVertexInputAttributeDescription normalAttribute = {};
	normalAttribute.binding = 0;
	normalAttribute.location = 1;
	normalAttribute.format = VK_FORMAT_R32G32B32_SFLOAT;
	normalAttribute.offset = offsetof( Vertex, normal );

	VkVertexInputAttributeDescription colorAttribute = {};
	colorAttribute.binding = 0;
	colorAttribute.location = 2;
	colorAttribute.format = VK_FORMAT_R32G32B32_SFLOAT;
	colorAttribute.offset = offsetof( Vertex, color );

	VkVertexInputAttributeDescription uvAttribute = {};
	uvAttribute.binding = 0;
	uvAttribute.location = 3;
	uvAttribute.format = VK_FORMAT_R32G32_SFLOAT;
	uvAttribute.offset = offsetof( Vertex, uv );

	VkVertexInputAttributeDescription tangentAttribute = {};
	tangentAttribute.binding = 0;
	tangentAttribute.location = 4;
	tangentAttribute.format = VK_FORMAT_R32G32B32_SFLOAT;
	tangentAttribute.offset = offsetof( Vertex, tangent );

	VkVertexInputAttributeDescription bitangentAttribute = {};
	bitangentAttribute.binding = 0;
	bitangentAttribute.location = 5;
	bitangentAttribute.format = VK_FORMAT_R32G32B32_SFLOAT;
	bitangentAttribute.offset = offsetof( Vertex, bitangent );	

	description.attributes.push_back( positionAttribute );
	description.attributes.push_back( normalAttribute );
	description.attributes.push_back( colorAttribute );
	description.attributes.push_back( uvAttribute );
	description.attributes.push_back( tangentAttribute );
	description.attributes.push_back( bitangentAttribute );

	return description;
}
