#include "modelentity.h"

void ModelEntity::Render( VkCommandBuffer cmd, Camera* camera )
{
	m_model.Render( camera, cmd );
}

void ModelEntity::SetModel( Model model )
{
	m_model = model;
}
