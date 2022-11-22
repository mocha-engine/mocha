#pragma once
#include <baseentity.h>
#include <game/model.h>
#include <vulkan/vulkan.h>

class ModelEntity : public BaseEntity
{
private:
	Model m_model;

public:
	void Render( VkCommandBuffer cmd, Camera* camera );
	void SetModel( Model model );
};
