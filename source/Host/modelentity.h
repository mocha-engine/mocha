#pragma once
#include <baseentity.h>
#include <game/model.h>
#include <vulkan/vulkan.h>

struct PhysicsBody;

class ModelEntity : public BaseEntity
{
private:
	Model m_model;

	//
	// Physics values
	//
	uint32_t m_physicsHandle = UINT32_MAX;

	Vector3 m_velocity = {};
	float m_friction = 0.5f;
	float m_mass = 10.0f;
	float m_restitution = 0.5f;

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

	float GetFriction() { return m_friction; }
	void SetFriction( float friction ) { m_friction = friction; }

	float GetMass() { return m_mass; }
	void SetMass( float mass ) { m_mass = mass; }

	float GetRestitution() { return m_restitution; }
	void SetRestitution( float restitution ) { m_restitution = restitution; }
};
