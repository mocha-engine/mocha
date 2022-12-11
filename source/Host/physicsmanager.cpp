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

#ifdef JPH_ENABLE_ASSERTS

// Callback for asserts
static bool AssertFailedImpl( const char* inExpression, const char* inMessage, const char* inFile, uint32_t inLine )
{
	spdlog::error( "{}: {} ({}:{})", inExpression, inMessage, inFile, inLine );

	// Breakpoint
	return true;
};

#endif // JPH_ENABLE_ASSERTS

void PhysicsManager::PreInit()
{
	// Register allocation hook
	JPH::RegisterDefaultAllocator();

	// Install callbacks
	JPH::Trace = TraceImpl;
	JPH_IF_ENABLE_ASSERTS( JPH::AssertFailed = AssertFailedImpl; )

	// Create a factory
	JPH::Factory::sInstance = new JPH::Factory();

	// Register all Jolt physics types
	JPH::RegisterTypes();
}

PhysicsManager::PhysicsManager()
{
	m_bodyIndex = 0;

	// We need a temp allocator for temporary allocations during the physics update. We're
	// pre-allocating 10 MB to avoid having to do allocations during the physics update.
	// B.t.w. 10 MB is way too much for this example but it is a typical value you can use.
	// If you don't want to pre-allocate you can also use TempAllocatorMalloc to fall back to
	// malloc / free.
	m_tempAllocator = new JPH::TempAllocatorImpl( 10 * 1024 * 1024 );

	// We need a job system that will execute physics jobs on multiple threads. Typically
	// you would implement the JobSystem interface yourself and let Jolt Physics run on top
	// of your own job scheduler. JobSystemThreadPool is an example implementation.
	m_jobSystem =
	    new JPH::JobSystemThreadPool( JPH::cMaxPhysicsJobs, JPH::cMaxPhysicsBarriers, JPH::thread::hardware_concurrency() - 1 );
}

void PhysicsManager::Startup()
{
	// This is the max amount of rigid bodies that you can add to the physics system. If you try to add more you'll get an
	// error. Note: This value is low because this is a simple test. For a real project use something in the order of 65536.
	const JPH::uint cMaxBodies = 1024;

	// This determines how many mutexes to allocate to protect rigid bodies from concurrent access. Set it to 0 for the default
	// settings.
	const JPH::uint cNumBodyMutexes = 0;

	// This is the max amount of body pairs that can be queued at any time (the broad phase will detect overlapping
	// body pairs based on their bounding boxes and will insert them into a queue for the narrowphase). If you make this buffer
	// too small the queue will fill up and the broad phase jobs will start to do narrow phase work. This is slightly less
	// efficient. Note: This value is low because this is a simple test. For a real project use something in the order of 65536.
	const JPH::uint cMaxBodyPairs = 1024;

	// This is the maximum size of the contact constraint buffer. If more contacts (collisions between bodies) are detected than
	// this number then these contacts will be ignored and bodies will start interpenetrating / fall through the world. Note:
	// This value is low because this is a simple test. For a real project use something in the order of 10240.
	const JPH::uint cMaxContactConstraints = 1024;

	// Now we can create the actual physics system.
	m_physicsSystem.Init( cMaxBodies, cNumBodyMutexes, cMaxBodyPairs, cMaxContactConstraints, m_broadPhaseLayerInterface,
	    MyBroadPhaseCanCollide, MyObjectCanCollide );

	spdlog::info( "Physics system has init" );
}

void PhysicsManager::Shutdown()
{
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

	// If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the
	// simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
	const int cCollisionSteps = 1;

	// If you want more accurate step results you can do multiple sub steps within a collision step. Usually you would set
	// this to 1.
	const int cIntegrationSubSteps = 1;

	// Step the world
	const float timeScale = 1.0f;
	m_physicsSystem.Update( g_frameTime * timeScale, cCollisionSteps, cIntegrationSubSteps, m_tempAllocator, m_jobSystem );

	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
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

	bool isStatic = body.type == PhysicsType::Static;
	JPH::EActivation activation = isStatic ? JPH::EActivation::DontActivate : JPH::EActivation::Activate;
	JPH::EMotionType motionType = isStatic ? JPH::EMotionType::Static : JPH::EMotionType::Dynamic;
	JPH::uint8 layer = isStatic ? Layers::NON_MOVING : Layers::MOVING;

	if ( body.shape.shapeType == PhysicsShapeType::Sphere )
	{
		// Create the settings for the collision volume (the shape).
		// Note that for simple shapes (like boxes) you can also directly construct a BoxShape.
		JPH::SphereShapeSettings sphere_shape_settings( body.shape.shapeData.radius );

		// Create the shape
		JPH::ShapeSettings::ShapeResult sphere_shape_result = sphere_shape_settings.Create();
		JPH::ShapeRefC sphere_shape =
		    sphere_shape_result
		        .Get(); // We don't expect an error here, but you can check sphere_shape_result for HasError() / GetError()

		// Create the settings for the body itself. Note that here you can also set other properties like the restitution /
		// friction.
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
		// Create the settings for the collision volume (the shape).
		// Note that for simple shapes (like boxes) you can also directly construct a BoxShape.
		
		// JOLT IS Y-UP! WE ARE Z-UP!
		auto extents =
		    JPH::Vec3( body.shape.shapeData.extents.x, body.shape.shapeData.extents.z, body.shape.shapeData.extents.y );

		JPH::BoxShapeSettings box_shape_settings( extents );

		// Create the shape
		JPH::ShapeSettings::ShapeResult box_shape_result = box_shape_settings.Create();
		JPH::ShapeRefC box_shape =
		    box_shape_result
		        .Get(); // We don't expect an error here, but you can check sphere_shape_result for HasError() / GetError()

		// Create the settings for the body itself. Note that here you can also set other properties like the restitution /
		// friction.
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