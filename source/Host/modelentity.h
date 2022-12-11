#pragma once
#include <baseentity.h>
#include <game/model.h>
#include <vulkan/vulkan.h>

struct PhysicsBody;

class ModelEntity : public BaseEntity
{
private:
	Model m_model;
	uint32_t m_physicsHandle = UINT32_MAX;
	Vector3 m_velocity;

public:
	void Render( VkCommandBuffer cmd, glm::mat4x4 viewProj ) override;

	void SetModel( Model model );

	//
	// Getters & setters
	//
	void SetSpherePhysics( float radius, bool isStatic );
	void SetCubePhysics( Vector3 bounds, bool isStatic );

	// If this model has no physics, this function will return UINT32_MAX.
	uint32_t GetPhysicsHandle() { return m_physicsHandle; };

	Vector3 GetVelocity() { return m_velocity; }
	void SetVelocity( Vector3 velocity ) { m_velocity = velocity; }
};
