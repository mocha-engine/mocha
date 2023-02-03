#include "modelentity.h"

#include <globalvars.h>
#include <physicsmanager.h>

void ModelEntity::SetSpherePhysics( float radius, bool isStatic )
{
	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = isStatic ? PhysicsType::PHYSICS_MODE_STATIC : PhysicsType::PHYSICS_MODE_DYNAMIC;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.radius = radius;
	body.shape.shapeType = PhysicsShapeType::PHYSICS_SHAPE_SPHERE;

	m_physicsHandle = g_physicsManager->AddBody( this, body );
}

void ModelEntity::SetCubePhysics( Vector3 bounds, bool isStatic )
{
	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = isStatic ? PhysicsType::PHYSICS_MODE_STATIC : PhysicsType::PHYSICS_MODE_DYNAMIC;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.extents = bounds;
	body.shape.shapeType = PhysicsShapeType::PHYSICS_SHAPE_BOX;

	m_physicsHandle = g_physicsManager->AddBody( this, body );
}

void ModelEntity::SetMeshPhysics( std::vector<Vector3> vertices )
{
	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = PhysicsType::PHYSICS_MODE_STATIC;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.vertices = vertices;
	body.shape.shapeType = PhysicsShapeType::PHYSICS_SHAPE_MESH;

	m_physicsHandle = g_physicsManager->AddBody( this, body );
}