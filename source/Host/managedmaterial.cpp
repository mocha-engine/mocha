#include "managedmaterial.h"

ManagedMaterial::ManagedMaterial( uint32_t vertexAttributeCount, void* vertexAttributes, ManagedTexture* diffuseTexture,
    ManagedTexture* normalTexture, ManagedTexture* ambientOcclusionTexture, ManagedTexture* metalnessTexture,
    ManagedTexture* roughnessTexture )
{
	m_diffuseTexture = diffuseTexture->GetTexture();
	m_normalTexture = normalTexture->GetTexture();
	m_ambientOcclusionTexture = ambientOcclusionTexture->GetTexture();
	m_metalnessTexture = metalnessTexture->GetTexture();
	m_roughnessTexture = roughnessTexture->GetTexture();

	m_vertexInputDescription = CreateVertexDescription( vertexAttributeCount, vertexAttributes );
}

Material ManagedMaterial::GetMaterial()
{
	return Material( m_vertexInputDescription, m_diffuseTexture, m_normalTexture, m_ambientOcclusionTexture, m_metalnessTexture,
	    m_roughnessTexture );
}

VkFormat ManagedMaterial::GetVulkanFormat( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VertexAttributeFormat::Int:
		return VK_FORMAT_R32_SINT;
		break;
	case VertexAttributeFormat::Float:
		return VK_FORMAT_R32_SFLOAT;
		break;
	case VertexAttributeFormat::Float2:
		return VK_FORMAT_R32G32_SFLOAT;
		break;
	case VertexAttributeFormat::Float3:
		return VK_FORMAT_R32G32B32_SFLOAT;
		break;
	case VertexAttributeFormat::Float4:
		return VK_FORMAT_R32G32B32A32_SFLOAT;
		break;
	}

	return VK_FORMAT_UNDEFINED;
}

size_t ManagedMaterial::GetSizeOf( VertexAttributeFormat format )
{
	switch ( format )
	{
	case VertexAttributeFormat::Int:
		return sizeof( int );
		break;
	case VertexAttributeFormat::Float:
		return sizeof( float );
		break;
	case VertexAttributeFormat::Float2:
		return sizeof( float ) * 2;
		break;
	case VertexAttributeFormat::Float3:
		return sizeof( float ) * 3;
		break;
	case VertexAttributeFormat::Float4:
		return sizeof( float ) * 4;
		break;
	}

	return 0;
}

VertexInputDescription ManagedMaterial::CreateVertexDescription( uint32_t count, void* data )
{
	std::vector<VertexAttribute> vertexAttributes;

	VertexAttribute* vertexAttribData = ( VertexAttribute* )data;

	vertexAttributes.insert( vertexAttributes.begin(), vertexAttribData, vertexAttribData + count );

	// Calculate stride size
	size_t stride = 0;
	for ( int i = 0; i < vertexAttributes.size(); ++i )
	{
		stride += GetSizeOf( ( VertexAttributeFormat )vertexAttributes[i].format );
	}

	VertexInputDescription description = {};

	VkVertexInputBindingDescription mainBinding = {};
	mainBinding.binding = 0;
	mainBinding.stride = stride;
	mainBinding.inputRate = VK_VERTEX_INPUT_RATE_VERTEX;

	description.bindings.push_back( mainBinding );

	size_t offset = 0;

	for ( int i = 0; i < vertexAttributes.size(); ++i )
	{
		auto attribute = vertexAttributes[i];

		VkVertexInputAttributeDescription positionAttribute = {};
		positionAttribute.binding = 0;
		positionAttribute.location = i;
		positionAttribute.format = GetVulkanFormat( ( VertexAttributeFormat )attribute.format );
		positionAttribute.offset = offset;
		description.attributes.push_back( positionAttribute );

		offset += GetSizeOf( ( VertexAttributeFormat )attribute.format );
	}

	return description;
}