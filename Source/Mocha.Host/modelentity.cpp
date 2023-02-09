#include "modelentity.h"

#include <physicsmanager.h>
#include <clientroot.h>

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
	
	m_physicsHandle = FindInstance()->m_physicsManager->AddBody( this, body );
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
	
	m_physicsHandle = FindInstance()->m_physicsManager->AddBody( this, body );
}

void ModelEntity::SetMeshPhysics( UtilArray interopVertices )
{
	std::vector<Vector3> vertices = interopVertices.GetData<Vector3>();

	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = PhysicsType::PHYSICS_MODE_STATIC;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.vertices = vertices;
	body.shape.shapeType = PhysicsShapeType::PHYSICS_SHAPE_MESH;
	
	m_physicsHandle = FindInstance()->m_physicsManager->AddBody( this, body );
}