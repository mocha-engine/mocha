#pragma once

#include <managedtexture.h>
#include <rendering.h>
#include <texture.h>

class Material;

//@InteropGen generate class
class ManagedMaterial
{
private:
	Texture m_diffuseTexture;
	Texture m_normalTexture;
	Texture m_ambientOcclusionTexture;
	Texture m_metalnessTexture;
	Texture m_roughnessTexture;

	VertexInputDescription m_vertexInputDescription;

	size_t GetSizeOf( VertexAttributeFormat format );
	VertexInputDescription CreateVertexDescription( uint32_t size, void* data );
	VkFormat GetVulkanFormat( VertexAttributeFormat format );

public:
	ManagedMaterial( uint32_t vertexAttributeCount, void* vertexAttributes, ManagedTexture* diffuseTexture,
	    ManagedTexture* normalTexture, ManagedTexture* ambientOcclusionTexture, ManagedTexture* metalnessTexture,
	    ManagedTexture* roughnessTexture );

	//@InteropGen ignore
	Material GetMaterial();
};
