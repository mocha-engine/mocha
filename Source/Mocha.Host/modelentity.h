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
	// If this model has no physics, this function will return UINT32_MAX.
	uint32_t GetPhysicsHandle() { return m_physicsHandle; };

	GENERATE_BINDINGS void SetModel( Model* model ) { m_model = *model; }
	GENERATE_BINDINGS Model* GetModel() { return &m_model; }

	//
	// Managed bindings
	//
	GENERATE_BINDINGS void SetSpherePhysics( float radius, bool isStatic );
	GENERATE_BINDINGS void SetCubePhysics( Vector3 bounds, bool isStatic );
	GENERATE_BINDINGS void SetMeshPhysics( UtilArray vertices );
	
	GENERATE_BINDINGS Vector3 GetVelocity() { return m_velocity; }
	GENERATE_BINDINGS void SetVelocity( Vector3 velocity ) { m_velocity = velocity; }

	GENERATE_BINDINGS float GetFriction() { return m_friction; }
	GENERATE_BINDINGS void SetFriction( float friction ) { m_friction = friction; }

	GENERATE_BINDINGS float GetMass() { return m_mass; }
	GENERATE_BINDINGS void SetMass( float mass ) { m_mass = mass; }

	GENERATE_BINDINGS float GetRestitution() { return m_restitution; }
	GENERATE_BINDINGS void SetRestitution( float restitution ) { m_restitution = restitution; }

	GENERATE_BINDINGS bool GetIgnoreRigidbodyRotation() { return m_ignoreRigidbodyRotation; }
	GENERATE_BINDINGS void SetIgnoreRigidbodyRotation( bool ignore ) { m_ignoreRigidbodyRotation = ignore; }

	GENERATE_BINDINGS bool GetIgnoreRigidbodyPosition() { return m_ignoreRigidbodyPosition; }
	GENERATE_BINDINGS void SetIgnoreRigidbodyPosition( bool ignore ) { m_ignoreRigidbodyPosition = ignore; }
};
