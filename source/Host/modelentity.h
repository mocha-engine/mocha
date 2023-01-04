#pragma once
#include <baseentity.h>
#include <mathtypes.h>
#include <model.h>
#include <vector>

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

	bool m_ignoreRigidbodyRotation;
	bool m_ignoreRigidbodyPosition;

public:
	void SetModel( Model model ) { m_model = model; }
	Model* GetModel() { return &m_model; }

	//
	// Getters & setters
	//
	void SetSpherePhysics( float radius, bool isStatic );
	void SetCubePhysics( Vector3 bounds, bool isStatic );
	void SetMeshPhysics( std::vector<Vector3> vertices );

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

	bool GetIgnoreRigidbodyRotation() { return m_ignoreRigidbodyRotation; }
	void SetIgnoreRigidbodyRotation( bool ignore ) { m_ignoreRigidbodyRotation = ignore; }

	bool GetIgnoreRigidbodyPosition() { return m_ignoreRigidbodyPosition; }
	void SetIgnoreRigidbodyPosition( bool ignore ) { m_ignoreRigidbodyPosition = ignore; }
};
