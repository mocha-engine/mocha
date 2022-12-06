#include "modelentity.h"

void ModelEntity::Render( VkCommandBuffer cmd, glm::mat4x4 viewProj )
{
	m_model.Render( cmd, viewProj, m_transform );
}

void ModelEntity::SetModel( Model model )
{
	m_model = model;
}
