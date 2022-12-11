#include "modelentity.h"

#include <globalvars.h>
#include <physicsmanager.h>

void ModelEntity::Render( VkCommandBuffer cmd, glm::mat4x4 viewProj )
{
	m_model.Render( cmd, viewProj, m_transform );
}

void ModelEntity::SetModel( Model model )
{
	m_model = model;
}

void ModelEntity::SetSpherePhysics( float radius, bool isStatic )
{
	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = isStatic ? PhysicsType::Static : PhysicsType::Dynamic;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.radius = radius;
	body.shape.shapeType = PhysicsShapeType::Sphere;

	m_physicsHandle = g_physicsManager->AddBody( this, body );
}

void ModelEntity::SetCubePhysics( Vector3 bounds, bool isStatic )
{
	PhysicsBody body = {};

	body.friction = 1.0f;
	body.restitution = 1.0f;

	body.transform = m_transform;
	body.type = isStatic ? PhysicsType::Static : PhysicsType::Dynamic;

	body.shape = {};
	body.shape.shapeData = {};
	body.shape.shapeData.extents = bounds;
	body.shape.shapeType = PhysicsShapeType::Box;

	m_physicsHandle = g_physicsManager->AddBody( this, body );
}
