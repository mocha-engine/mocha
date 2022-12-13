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

	m_physicsSystem.SetGravity( JPH::Vec3( 0, 0, -9.8f ) );

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

		JPH::Vec3 velocity = JoltConversions::MochaToJoltVec3( savedVelocity );
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

		if ( !modelEntity->GetIgnoreRigidbodyPosition() )
			tx.position = JoltConversions::JoltToMochaVec3( position );
		else
			bodyInterface.SetPosition(
			    body->bodyId, JoltConversions::MochaToJoltVec3( tx.position ), JPH::EActivation::Activate );

		if ( !modelEntity->GetIgnoreRigidbodyRotation() )
			tx.rotation = JoltConversions::JoltToMochaQuat( rotation );
		else
			bodyInterface.SetRotation(
			    body->bodyId, JoltConversions::MochaToJoltQuat( tx.rotation ), JPH::EActivation::Activate );

		modelEntity->SetTransform( tx );

		// Save off velocity so that we can make changes to it if we need to
		modelEntity->SetVelocity( JoltConversions::JoltToMochaVec3( velocity ) );
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

	auto transform = entity->GetTransform();
	auto position = JoltConversions::MochaToJoltVec3( transform.position );
	auto rotation = JoltConversions::MochaToJoltQuat( transform.rotation );

	JPH::ShapeSettings* shapeSettings = nullptr;

	// Shape-specific setup
	switch ( body.shape.shapeType )
	{
	case PhysicsShapeType::Sphere:
		shapeSettings = new JPH::SphereShapeSettings( body.shape.shapeData.radius );
		break;

	case PhysicsShapeType::Box:
		auto extents = JoltConversions::MochaToJoltVec3( body.shape.shapeData.extents );
		shapeSettings = new JPH::BoxShapeSettings( extents );
		break;

	default:
		spdlog::error( "Unsupported phys shape" );
		return UINT32_MAX;
	}

	// Make a shape, then set up a body
	JPH::ShapeSettings::ShapeResult shapeResult = shapeSettings->Create();
	JPH::ShapeRefC shape = shapeResult.Get();

	JPH::BodyCreationSettings bodyCreationSettings( shape, position, rotation, motionType, layer );

	// Assign values from the entity
	float restitution = entity->GetRestitution();
	float friction = entity->GetFriction();
	float mass = entity->GetMass();

	bodyCreationSettings.mRestitution = restitution;
	bodyCreationSettings.mFriction = friction;
	bodyCreationSettings.mMassPropertiesOverride = JPH::MassProperties();
	bodyCreationSettings.mMassPropertiesOverride.mMass = mass;

	// Create the actual rigid body, add it to the world
	body.bodyId = bodyInterface.CreateAndAddBody( bodyCreationSettings, activation );

	return Add( body );
}

TraceResult PhysicsManager::Trace( TraceInfo traceInfo )
{
	if ( traceInfo.isBox )
		return TraceBox( traceInfo );

	return TraceRay( traceInfo );
}

bool PhysicsManager::IsBodyIgnored( TraceInfo& traceInfo, JPH::BodyID bodyId )
{
	//
	// We need to do the following steps:
	// 1: JPH body -> physicsmanager handle
	// 2: physicsmanager handle -> entity handle
	// 3: entity handle == traceInfo.ignoredEntityList[i]?
	//

	//
	// Step 1: find physmanager handle
	//
	Handle physHandle = UINT32_MAX;
	For( [&]( Handle handle, std::shared_ptr<PhysicsBody> object ) {
		if ( object->bodyId == bodyId )
			physHandle = handle;
	} );

	// Did we find one?
	if ( physHandle == UINT32_MAX )
		return true;

	//
	// Step 2: find entity handle
	//
	Handle entHandle = UINT32_MAX;
	g_entityDictionary->For( [&]( Handle handle, std::shared_ptr<BaseEntity> entity ) {
		auto modelEntity = std::dynamic_pointer_cast<ModelEntity>( entity );

		if ( modelEntity == nullptr )
			return;

		if ( modelEntity->GetPhysicsHandle() == physHandle )
			entHandle = handle;
	} );

	// Did we find one?
	if ( entHandle == UINT32_MAX )
		return true;

	//
	// Step 3: check if physics handle is part of the ignored list
	//
	for ( size_t i = 0; i < traceInfo.ignoredEntityCount; i++ )
	{
		// Is this an ignored entity?
		if ( traceInfo.ignoredEntityHandles[i] == entHandle )
			return true;
	}
	
	// This body is NOT ignored, we can use it for this trace.
	return false;
}

TraceResult PhysicsManager::TraceRay( TraceInfo traceInfo )
{
	// Initial trace result properties - we use a lot of these if we don't hit anything.
	TraceResult traceResult = {};
	traceResult.startPosition = traceInfo.startPosition;
	traceResult.endPosition = traceInfo.endPosition;
	traceResult.fraction = 1.0f;

	const JPH::NarrowPhaseQuery& sceneQuery = m_physicsSystem.GetNarrowPhaseQuery();

	auto origin = JoltConversions::MochaToJoltVec3( traceInfo.startPosition );
	auto direction = JoltConversions::MochaToJoltVec3( traceInfo.endPosition ) - origin;

	// Create ray cast
	JPH::RayCast ray = {};
	ray.mOrigin = origin;
	ray.mDirection = direction;

	JPH::RayCastSettings rayCastSettings;

	JPH::AllHitCollisionCollector<JPH::CastRayCollector> collector;
	sceneQuery.CastRay( ray, rayCastSettings, collector );

	auto& bodyInterface = m_physicsSystem.GetBodyInterface();

	// Did we hit anything at all? If not, bail now
	if ( !collector.HadHit() )
	{
		traceResult.hit = false;
		return traceResult;
	}

	// We might have hit something, let's do some filtering
	collector.Sort();

	std::vector<JPH::RayCastResult> raycastResults( collector.mHits.begin(), collector.mHits.end() );

	// Find the first raycast result that matches our parameters
	for ( size_t i = 0; i < raycastResults.size(); i++ )
	{
		const JPH::RayCastResult& result = raycastResults[i];

		auto bodyID = result.mBodyID.GetIndexAndSequenceNumber();
		JPH::BodyLockRead bodyLock( m_physicsSystem.GetBodyLockInterface(), result.mBodyID );
		const JPH::Body& hitBody = bodyLock.GetBody();

		// Is this body ignored in the trace filter?
		if ( IsBodyIgnored( traceInfo, result.mBodyID ) )
			continue;

		//
		// We got this far - that means that the entity we've hit is valid and
		// not part of the ignored list. Let's return some relevant values.
		//
		traceResult.hit = true;

		// Calculate end position
		traceResult.endPosition = JoltConversions::JoltToMochaVec3( ray.mOrigin + result.mFraction * ray.mDirection );

		// Hit fraction
		traceResult.fraction = result.mFraction;

		// Calculate hit normal
		traceResult.normal = JoltConversions::JoltToMochaVec3( hitBody.GetWorldSpaceSurfaceNormal(
		    result.mSubShapeID2, JoltConversions::MochaToJoltVec3( traceResult.endPosition ) ) );

		return traceResult;
	}

	return traceResult;
}

TraceResult PhysicsManager::TraceBox( TraceInfo traceInfo )
{
	// Initial trace result properties - we use a lot of these if we don't hit anything.
	TraceResult traceResult = {};
	traceResult.startPosition = traceInfo.startPosition;
	traceResult.endPosition = traceInfo.endPosition;
	traceResult.fraction = 1.0f;

	// TODO

	return traceResult;
}