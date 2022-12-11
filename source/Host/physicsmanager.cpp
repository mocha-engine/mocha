#include "physicsmanager.h"

#include <Jolt/Jolt.h>
#include <spdlog/spdlog.h>

// Jolt includes
#include <Jolt/Core/Factory.h>
#include <Jolt/Core/JobSystemThreadPool.h>
#include <Jolt/Core/TempAllocator.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Collision/BroadPhase/BroadPhaseLayer.h>
#include <Jolt/Physics/Collision/ContactListener.h>
#include <Jolt/Physics/Collision/ObjectLayer.h>
#include <Jolt/Physics/Collision/Shape/BoxShape.h>
#include <Jolt/Physics/Collision/Shape/SphereShape.h>
#include <Jolt/Physics/PhysicsSettings.h>
#include <Jolt/Physics/PhysicsSystem.h>
#include <Jolt/RegisterTypes.h>

// STL includes
#include <cstdarg>
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

// Layer that objects can be in, determines which other objects it can collide with
// Typically you at least want to have 1 layer for moving bodies and 1 layer for static bodies, but you can have more
// layers if you want. E.g. you could have a layer for high detail collision (which is not used by the physics simulation
// but only if you do collision testing).
namespace Layers
{
	static constexpr JPH::uint8 NON_MOVING = 0;
	static constexpr JPH::uint8 MOVING = 1;
	static constexpr JPH::uint8 NUM_LAYERS = 2;
}; // namespace Layers

// Function that determines if two object layers can collide
static bool MyObjectCanCollide( JPH::ObjectLayer inObject1, JPH::ObjectLayer inObject2 )
{
	switch ( inObject1 )
	{
	case Layers::NON_MOVING:
		return inObject2 == Layers::MOVING; // Non moving only collides with moving
	case Layers::MOVING:
		return true; // Moving collides with everything
	default:
		assert( false );
		return false;
	}
};

// Each broadphase layer results in a separate bounding volume tree in the broad phase. You at least want to have
// a layer for non-moving and moving objects to avoid having to update a tree full of static objects every frame.
// You can have a 1-on-1 mapping between object layers and broadphase layers (like in this case) but if you have
// many object layers you'll be creating many broad phase trees, which is not efficient. If you want to fine tune
// your broadphase layers define JPH_TRACK_BROADPHASE_STATS and look at the stats reported on the TTY.
namespace BroadPhaseLayers
{
	static constexpr JPH::BroadPhaseLayer NON_MOVING( 0 );
	static constexpr JPH::BroadPhaseLayer MOVING( 1 );
	static constexpr JPH::uint NUM_LAYERS( 2 );
}; // namespace BroadPhaseLayers

// BroadPhaseLayerInterface implementation
// This defines a mapping between object and broadphase layers.
class BPLayerInterfaceImpl final : public JPH::BroadPhaseLayerInterface
{
public:
	BPLayerInterfaceImpl()
	{
		// Create a mapping table from object to broad phase layer
		mObjectToBroadPhase[Layers::NON_MOVING] = BroadPhaseLayers::NON_MOVING;
		mObjectToBroadPhase[Layers::MOVING] = BroadPhaseLayers::MOVING;
	}

	virtual JPH::uint GetNumBroadPhaseLayers() const override { return BroadPhaseLayers::NUM_LAYERS; }

	virtual JPH::BroadPhaseLayer GetBroadPhaseLayer( JPH::ObjectLayer inLayer ) const override
	{
		assert( inLayer < Layers::NUM_LAYERS );
		return mObjectToBroadPhase[inLayer];
	}

#if defined( JPH_EXTERNAL_PROFILE ) || defined( JPH_PROFILE_ENABLED )
	virtual const char* GetBroadPhaseLayerName( BroadPhaseLayer inLayer ) const override
	{
		switch ( ( BroadPhaseLayer::Type )inLayer )
		{
		case ( BroadPhaseLayer::Type )BroadPhaseLayers::NON_MOVING:
			return "NON_MOVING";
		case ( BroadPhaseLayer::Type )BroadPhaseLayers::MOVING:
			return "MOVING";
		default:
			JPH_ASSERT( false );
			return "INVALID";
		}
	}
#endif // JPH_EXTERNAL_PROFILE || JPH_PROFILE_ENABLED

private:
	JPH::BroadPhaseLayer mObjectToBroadPhase[Layers::NUM_LAYERS];
};

// Function that determines if two broadphase layers can collide
static bool MyBroadPhaseCanCollide( JPH::ObjectLayer inLayer1, JPH::BroadPhaseLayer inLayer2 )
{
	switch ( inLayer1 )
	{
	case Layers::NON_MOVING:
		return inLayer2 == BroadPhaseLayers::MOVING;
	case Layers::MOVING:
		return true;
	default:
		assert( false );
		return false;
	}
}

// An example contact listener
class MyContactListener : public JPH::ContactListener
{
public:
	// See: ContactListener
	virtual JPH::ValidateResult OnContactValidate(
	    const JPH::Body& inBody1, const JPH::Body& inBody2, const JPH::CollideShapeResult& inCollisionResult ) override
	{
		spdlog::info( "Contact validate callback" );

		// Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
		return JPH::ValidateResult::AcceptAllContactsForThisBodyPair;
	}

	virtual void OnContactAdded( const JPH::Body& inBody1, const JPH::Body& inBody2, const JPH::ContactManifold& inManifold,
	    JPH::ContactSettings& ioSettings ) override
	{
		spdlog::info( "A contact was added" );
	}

	virtual void OnContactPersisted( const JPH::Body& inBody1, const JPH::Body& inBody2, const JPH::ContactManifold& inManifold,
	    JPH::ContactSettings& ioSettings ) override
	{
		spdlog::info( "A contact was persisted" );
	}

	virtual void OnContactRemoved( const JPH::SubShapeIDPair& inSubShapePair ) override
	{
		spdlog::info( "A contact was removed" );
	}
};

// An example activation listener
class MyBodyActivationListener : public JPH::BodyActivationListener
{
public:
	virtual void OnBodyActivated( const JPH::BodyID& inBodyID, JPH::uint64 inBodyUserData ) override
	{
		spdlog::info( "A body got activated" );
	}

	virtual void OnBodyDeactivated( const JPH::BodyID& inBodyID, JPH::uint64 inBodyUserData ) override
	{
		spdlog::info( "A body went to sleep" );
	}
};

void PhysicsManager::Startup()
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

	// We need a temp allocator for temporary allocations during the physics update. We're
	// pre-allocating 10 MB to avoid having to do allocations during the physics update.
	// B.t.w. 10 MB is way too much for this example but it is a typical value you can use.
	// If you don't want to pre-allocate you can also use TempAllocatorMalloc to fall back to
	// malloc / free.
	JPH::TempAllocatorImpl temp_allocator( 10 * 1024 * 1024 );

	// We need a job system that will execute physics jobs on multiple threads. Typically
	// you would implement the JobSystem interface yourself and let Jolt Physics run on top
	// of your own job scheduler. JobSystemThreadPool is an example implementation.
	JPH::JobSystemThreadPool job_system(
	    JPH::cMaxPhysicsJobs, JPH::cMaxPhysicsBarriers, JPH::thread::hardware_concurrency() - 1 );

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

	// Create mapping table from object layer to broadphase layer
	// Note: As this is an interface, PhysicsSystem will take a reference to this so this instance needs to stay alive!
	BPLayerInterfaceImpl broad_phase_layer_interface;

	// Now we can create the actual physics system.
	JPH::PhysicsSystem physics_system;
	physics_system.Init( cMaxBodies, cNumBodyMutexes, cMaxBodyPairs, cMaxContactConstraints, broad_phase_layer_interface,
	    MyBroadPhaseCanCollide, MyObjectCanCollide );

	// A body activation listener gets notified when bodies activate and go to sleep
	// Note that this is called from a job so whatever you do here needs to be thread safe.
	// Registering one is entirely optional.
	MyBodyActivationListener body_activation_listener;
	physics_system.SetBodyActivationListener( &body_activation_listener );

	// A contact listener gets notified when bodies (are about to) collide, and when they separate again.
	// Note that this is called from a job so whatever you do here needs to be thread safe.
	// Registering one is entirely optional.
	MyContactListener contact_listener;
	physics_system.SetContactListener( &contact_listener );

	// The main way to interact with the bodies in the physics system is through the body interface. There is a locking and a
	// non-locking variant of this. We're going to use the locking version (even though we're not planning to access bodies from
	// multiple threads)
	JPH::BodyInterface& body_interface = physics_system.GetBodyInterface();

	// Next we can create a rigid body to serve as the floor, we make a large box
	// Create the settings for the collision volume (the shape).
	// Note that for simple shapes (like boxes) you can also directly construct a BoxShape.
	JPH::BoxShapeSettings floor_shape_settings( JPH::Vec3( 100.0f, 1.0f, 100.0f ) );

	// Create the shape
	JPH::ShapeSettings::ShapeResult floor_shape_result = floor_shape_settings.Create();
	JPH::ShapeRefC floor_shape =
	    floor_shape_result
	        .Get(); // We don't expect an error here, but you can check floor_shape_result for HasError() / GetError()

	// Create the settings for the body itself. Note that here you can also set other properties like the restitution /
	// friction.
	JPH::BodyCreationSettings floor_settings(
	    floor_shape, JPH::Vec3( 0.0f, -1.0f, 0.0f ), JPH::Quat::sIdentity(), JPH::EMotionType::Static, Layers::NON_MOVING );

	// Create the actual rigid body
	JPH::Body* floor = body_interface.CreateBody( floor_settings ); // Note that if we run out of bodies this can return nullptr

	// Add it to the world
	body_interface.AddBody( floor->GetID(), JPH::EActivation::DontActivate );

	// Now create a dynamic body to bounce on the floor
	// Note that this uses the shorthand version of creating and adding a body to the world
	JPH::BodyCreationSettings sphere_settings( new JPH::SphereShape( 0.5f ), JPH::Vec3( 0.0f, 2.0f, 0.0f ),
	    JPH::Quat::sIdentity(), JPH::EMotionType::Dynamic, Layers::MOVING );
	JPH::BodyID sphere_id = body_interface.CreateAndAddBody( sphere_settings, JPH::EActivation::Activate );

	// Now you can interact with the dynamic body, in this case we're going to give it a velocity.
	// (note that if we had used CreateBody then we could have set the velocity straight on the body before adding it to the
	// physics system)
	body_interface.SetLinearVelocity( sphere_id, JPH::Vec3( 0.0f, -5.0f, 0.0f ) );

	// We simulate the physics world in discrete time steps. 60 Hz is a good rate to update the physics system.
	const float cDeltaTime = 1.0f / 60.0f;

	// Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection
	// performance (it's pointless here because we only have 2 bodies). You should definitely not call this every frame or when
	// e.g. streaming in a new level section as it is an expensive operation. Instead insert all new objects in batches instead
	// of 1 at a time to keep the broad phase efficient.
	physics_system.OptimizeBroadPhase();

	// Now we're ready to simulate the body, keep simulating until it goes to sleep
	JPH::uint step = 0;
	while ( body_interface.IsActive( sphere_id ) )
	{
		// Next step
		++step;

		// Output current position and velocity of the sphere
		JPH::Vec3 position = body_interface.GetCenterOfMassPosition( sphere_id );
		JPH::Vec3 velocity = body_interface.GetLinearVelocity( sphere_id );
		spdlog::trace( "Step {}: Position = ({}, {}, {}), Velocity = ({}, {}, {})", step, position.GetX(), position.GetY(),
		    position.GetZ(), velocity.GetX(), velocity.GetY(), velocity.GetZ() );

		// If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the
		// simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
		const int cCollisionSteps = 1;

		// If you want more accurate step results you can do multiple sub steps within a collision step. Usually you would set
		// this to 1.
		const int cIntegrationSubSteps = 1;

		// Step the world
		physics_system.Update( cDeltaTime, cCollisionSteps, cIntegrationSubSteps, &temp_allocator, &job_system );
	}

	// Remove the sphere from the physics system. Note that the sphere itself keeps all of its state and can be re-added at any
	// time.
	body_interface.RemoveBody( sphere_id );

	// Destroy the sphere. After this the sphere ID is no longer valid.
	body_interface.DestroyBody( sphere_id );

	// Remove and destroy the floor
	body_interface.RemoveBody( floor->GetID() );
	body_interface.DestroyBody( floor->GetID() );

	// Destroy the factory
	delete JPH::Factory::sInstance;
	JPH::Factory::sInstance = nullptr;
}

void PhysicsManager::Shutdown() {}

void PhysicsManager::Update() {}

uint32_t PhysicsManager::AddBody( PhysicsBody body )
{
	// Create a shared pointer to the body.
	auto bodyPtr = std::make_shared<PhysicsBody>( body );

	// Add the entity to the map.
	m_bodies[m_bodyIndex] = bodyPtr;

	return m_bodyIndex++;
}

Transform* PhysicsManager::GetTransform( uint32_t bodyHandle )
{
	return &m_bodies[bodyHandle].get()->transform;
}
