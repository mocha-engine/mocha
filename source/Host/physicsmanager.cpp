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

	// Retrieve velocities that were saved off last frame
	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		// Is this a valid entity to do physics stuff on?
		auto modelEntity = std::dynamic_pointer_cast<ModelEntity>( entity );

		if ( modelEntity == nullptr )
			return;

		auto physicsHandle = modelEntity->GetPhysicsHandle();

		if ( physicsHandle == UINT32_MAX )
			return;

		auto body = Get( physicsHandle );
		auto savedVelocity = modelEntity->GetVelocity();

		JPH::Vec3 velocity = MochaToJoltVec3( savedVelocity );
		bodyInterface.SetLinearVelocity( body->bodyId, velocity );
	} );

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

		auto body = Get( physicsHandle );

		// Get properties & assign them to the model entity's transform.
		JPH::Vec3 position = bodyInterface.GetCenterOfMassPosition( body->bodyId );
		JPH::Quat rotation = bodyInterface.GetRotation( body->bodyId );
		JPH::Vec3 velocity = bodyInterface.GetLinearVelocity( body->bodyId );

		Transform tx = modelEntity->GetTransform();

		tx.position = JoltToMochaVec3( position );
		tx.rotation = JoltToMochaQuat( rotation );

		modelEntity->SetTransform( tx );

		// Save off velocity so that we can make changes to it if we need to
		modelEntity->SetVelocity( JoltToMochaVec3( velocity ) );
	} );
}

uint32_t PhysicsManager::AddBody( ModelEntity* entity, PhysicsBody body )
{
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

		JPH::ShapeSettings::ShapeResult sphereShapeResult = sphere_shape_settings.Create();
		JPH::ShapeRefC sphereShape = sphereShapeResult.Get();

		auto transform = entity->GetTransform();

		auto position = MochaToJoltVec3( transform.position );
		auto rotation = MochaToJoltQuat( transform.rotation );

		JPH::BodyCreationSettings sphereSettings( sphereShape, position, rotation, motionType, layer );

		sphereSettings.mRestitution = 0.9f;

		// Add it to the world
		body.bodyId = bodyInterface.CreateAndAddBody( sphereSettings, activation );
	}

	if ( body.shape.shapeType == PhysicsShapeType::Box )
	{
		// Create the settings for a box, then create and add a rigid body.
		auto extents = MochaToJoltVec3( body.shape.shapeData.extents );

		JPH::BoxShapeSettings box_shape_settings( extents );

		JPH::ShapeSettings::ShapeResult boxShapeResult = box_shape_settings.Create();
		JPH::ShapeRefC boxShape = boxShapeResult.Get();

		auto transform = entity->GetTransform();

		auto position = MochaToJoltVec3( transform.position );
		auto rotation = MochaToJoltQuat( transform.rotation );
		JPH::BodyCreationSettings boxSettings( boxShape, position, rotation, motionType, layer );

		boxSettings.mRestitution = 0.9f;

		// Create the actual rigid body
		body.bodyId = bodyInterface.CreateAndAddBody( boxSettings, activation );
	}

	return Add( body );
}

TraceResult PhysicsManager::TraceRay( Vector3 startPosition, Vector3 endPosition )
{
	// Initial trace result properties - we use a lot of these if we don't hit anything.
	TraceResult traceResult = {};
	traceResult.startPosition = startPosition;
	traceResult.endPosition = endPosition;
	traceResult.fraction = 1.0f;

	const JPH::NarrowPhaseQuery& sceneQuery = m_physicsSystem.GetNarrowPhaseQuery();

	// Create ray cast
	JPH::RayCast ray = {};
	ray.mOrigin = MochaToJoltVec3( startPosition );
	ray.mDirection = MochaToJoltVec3( endPosition ) - ray.mOrigin;

	JPH::RayCastSettings rayCastSettings;

	JPH::AllHitCollisionCollector<JPH::CastRayCollector> collector;
	sceneQuery.CastRay( ray, rayCastSettings, collector );

	auto& bodyInterface = m_physicsSystem.GetBodyInterface();

	// Did we hit? If not, bail now
	if ( !collector.HadHit() )
	{
		traceResult.hit = false;
		return traceResult;
	}

	// We hit something so let's return some relevant values
	traceResult.hit = true;
	collector.Sort();

	std::vector<JPH::RayCastResult> raycastResults( collector.mHits.begin(), collector.mHits.end() );
	const JPH::RayCastResult& mainResult = raycastResults[0];

	// Calculate end position
	traceResult.endPosition = JoltToMochaVec3( ray.mOrigin + mainResult.mFraction * ray.mDirection );

	// Hit fraction
	traceResult.fraction = mainResult.mFraction;

	// Calculate hit normal
	auto bodyID = mainResult.mBodyID.GetIndexAndSequenceNumber();
	JPH::BodyLockRead body_lock( m_physicsSystem.GetBodyLockInterface(), mainResult.mBodyID );
	const JPH::Body& hit_body = body_lock.GetBody();

	traceResult.normal = JoltToMochaVec3(
	    hit_body.GetWorldSpaceSurfaceNormal( mainResult.mSubShapeID2, MochaToJoltVec3( traceResult.endPosition ) ) );

	return traceResult;
}
