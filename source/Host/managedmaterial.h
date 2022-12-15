#pragma once

#include <managedtexture.h>
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

public:
	ManagedMaterial( ManagedTexture* diffuseTexture, ManagedTexture* normalTexture, ManagedTexture* ambientOcclusionTexture,
	    ManagedTexture* metalnessTexture, ManagedTexture* roughnessTexture );

	//@InteropGen ignore
	Material GetMaterial();
};
