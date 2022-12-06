#pragma once
#include <baseentity.h>
#include <game/model.h>
#include <vulkan/vulkan.h>

class ModelEntity : public BaseEntity
{
private:
	Model m_model;

public:
	void Render( VkCommandBuffer cmd, glm::mat4x4 viewProj ) override;
	
	void SetModel( Model model );
};
