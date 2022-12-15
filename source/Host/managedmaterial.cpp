#include "managedmaterial.h"

ManagedMaterial::ManagedMaterial( ManagedTexture* diffuseTexture, ManagedTexture* normalTexture,
    ManagedTexture* ambientOcclusionTexture, ManagedTexture* metalnessTexture, ManagedTexture* roughnessTexture )
{
	m_diffuseTexture = diffuseTexture->GetTexture();
	m_normalTexture = normalTexture->GetTexture();
	m_ambientOcclusionTexture = ambientOcclusionTexture->GetTexture();
	m_metalnessTexture = metalnessTexture->GetTexture();
	m_roughnessTexture = roughnessTexture->GetTexture();
}

Material ManagedMaterial::GetMaterial()
{
	return Material( m_diffuseTexture, m_normalTexture, m_ambientOcclusionTexture, m_metalnessTexture, m_roughnessTexture );
}
