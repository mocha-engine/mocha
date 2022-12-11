#include "physicsmanager.h"

// STL includes
#include <cstdarg>
#include <edict.h>
#include <globalvars.h>
#include <iostream>
#include <thread>

// Callback for traces
static void TraceImpl( const char* inFMT, ... )
{
	// Format the message
	va_list list;
	va_start( list, inFMT );
	char buffer[1024];
	vsnprintf( buffer, sizeof( buffer ), inFMT, list );
	va_end( list );

	spdlog::info( "{}", buffer );
}

void PhysicsManager::PreInit()
{
	JPH::RegisterDefaultAllocator();
	JPH::Trace = TraceImpl;
	JPH::Factory::sInstance = new JPH::Factory();
	JPH::RegisterTypes();
}

PhysicsManager::PhysicsManager()
{
	m_bodyIndex = 0;

	m_tempAllocator = new JPH::TempAllocatorImpl( 10 * 1024 * 1024 );

	// JobSystemThreadPool is an example implementation.
	m_jobSystem =
	    new JPH::JobSystemThreadPool( JPH::cMaxPhysicsJobs, JPH::cMaxPhysicsBarriers, JPH::thread::hardware_concurrency() - 1 );
}

void PhysicsManager::Startup()
{
	const JPH::uint numBodyMutexes = 0;
	const JPH::uint maxBodyPairs = 65536;
	const JPH::uint maxContactConstraints = 10240;
	const JPH::uint maxBodies = 65536;

	// Create the actual physics system.
	m_physicsSystem.Init( maxBodies, numBodyMutexes, maxBodyPairs, maxContactConstraints, m_broadPhaseLayerInterface,
	    MyBroadPhaseCanCollide, MyObjectCanCollide );

	spdlog::info( "Physics system has init" );
}

void PhysicsManager::Shutdown()
{
	// TODO: Destroy and remove all bodies
	// body_interface.RemoveBody( sphere_id );
	// body_interface.DestroyBody( sphere_id );
	// ...

	// Destroy the factory
	delete JPH::Factory::sInstance;
	JPH::Factory::sInstance = nullptr;
}

void PhysicsManager::Update()
{
	auto& bodyInterface = m_physicsSystem.GetBodyInterface();

	// We will default to 4 but this should be 1 collision step per 1 / 60th of a second (round up).
	const int collisionSteps = 4;
	const int integrationSubSteps = 1;

	// Step the world
	const float timeScale = 1.0f;
	m_physicsSystem.Update( g_frameTime * timeScale, collisionSteps, integrationSubSteps, m_tempAllocator, m_jobSystem );

	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		// Is this a valid entity to do physics stuff on?
		auto modelEntity = std::dynamic_pointer_cast<ModelEntity>( entity );

		if ( modelEntity == nullptr )
			return;

		auto physicsHandle = modelEntity->GetPhysicsHandle();

		if ( physicsHandle == UINT32_MAX )
			return;

		auto body = m_bodies[physicsHandle].get();

		// Get properties & assign them to the model entity's transform.
		JPH::Vec3 position = bodyInterface.GetCenterOfMassPosition( body->bodyId );
		JPH::Quat rotation = bodyInterface.GetRotation( body->bodyId );
		JPH::Vec3 velocity = bodyInterface.GetLinearVelocity( body->bodyId );

		Transform tx = modelEntity->GetTransform();

		// JOLT IS Y-UP! WE ARE Z-UP!
		// TODO: should probably make a MochaToJoltVec3 method that handles this..
		tx.position = { position.GetX(), position.GetZ(), position.GetY() };
		tx.rotation = { rotation.GetX(), rotation.GetY(), rotation.GetZ(), rotation.GetW() };

		modelEntity->SetTransform( tx );
	} );
}

uint32_t PhysicsManager::AddBody( ModelEntity* entity, PhysicsBody body )
{
	// Create a shared pointer to the body.
	auto bodyPtr = std::make_shared<PhysicsBody>( body );

	// Add the entity to the map.
	m_bodies[m_bodyIndex] = bodyPtr;

	// Add the body to the physics world
	auto& bodyInterface = m_physicsSystem.GetBodyInterface();

	// These basic properties are used across all types - they're just based on whether we want the physics body
	// to move and interact with things or not.
	bool isStatic = body.type == PhysicsType::Static;
	JPH::EActivation activation = isStatic ? JPH::EActivation::DontActivate : JPH::EActivation::Activate;
	JPH::EMotionType motionType = isStatic ? JPH::EMotionType::Static : JPH::EMotionType::Dynamic;
	JPH::uint8 layer = isStatic ? Layers::NON_MOVING : Layers::MOVING;

	if ( body.shape.shapeType == PhysicsShapeType::Sphere )
	{
		// Create the settings for a sphere, then create and add a rigid body.
		JPH::SphereShapeSettings sphere_shape_settings( body.shape.shapeData.radius );

		JPH::ShapeSettings::ShapeResult sphere_shape_result = sphere_shape_settings.Create();
		JPH::ShapeRefC sphere_shape = sphere_shape_result.Get();

		auto transform = entity->GetTransform();

		// JOLT IS Y-UP! WE ARE Z-UP!
		auto position = JPH::Vec3( transform.position.x, transform.position.z, transform.position.y );
		auto rotation = JPH::Quat( transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w );

		JPH::BodyCreationSettings sphere_settings( sphere_shape, position, rotation, motionType, layer );

		sphere_settings.mRestitution = 0.9f;

		// Add it to the world
		bodyPtr->bodyId = bodyInterface.CreateAndAddBody( sphere_settings, activation );
	}

	if ( body.shape.shapeType == PhysicsShapeType::Box )
	{
		// Create the settings for a box, then create and add a rigid body.

		// JOLT IS Y-UP! WE ARE Z-UP!
		auto extents =
		    JPH::Vec3( body.shape.shapeData.extents.x, body.shape.shapeData.extents.z, body.shape.shapeData.extents.y );

		JPH::BoxShapeSettings box_shape_settings( extents );

		JPH::ShapeSettings::ShapeResult box_shape_result = box_shape_settings.Create();
		JPH::ShapeRefC box_shape = box_shape_result.Get();

		auto transform = entity->GetTransform();

		// JOLT IS Y-UP! WE ARE Z-UP!
		auto position = JPH::Vec3( transform.position.x, transform.position.z, transform.position.y );
		auto rotation = JPH::Quat( transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w );
		JPH::BodyCreationSettings box_settings( box_shape, position, rotation, motionType, layer );

		box_settings.mRestitution = 0.9f;

		// Create the actual rigid body
		bodyPtr->bodyId = bodyInterface.CreateAndAddBody( box_settings, activation );
	}

	return m_bodyIndex++;
}