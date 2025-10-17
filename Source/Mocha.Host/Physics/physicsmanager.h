#pragma once
#include <Entities/modelentity.h>
#include <Jolt/Jolt.h>
#include <Misc/handlemap.h>
#include <Misc/mathtypes.h>
#include <Misc/subsystem.h>
#include <atomic>
#include <memory>
#include <spdlog/spdlog.h>
#include <unordered_map>

// Jolt includes
#include <Jolt/Core/Factory.h>
#include <Jolt/Core/JobSystemThreadPool.h>
#include <Jolt/Core/TempAllocator.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Collision/BroadPhase/BroadPhaseLayer.h>
#include <Jolt/Physics/Collision/CastResult.h>
#include <Jolt/Physics/Collision/CollisionCollector.h>
#include <Jolt/Physics/Collision/CollisionCollectorImpl.h>
#include <Jolt/Physics/Collision/ContactListener.h>
#include <Jolt/Physics/Collision/ObjectLayer.h>
#include <Jolt/Physics/Collision/RayCast.h>
#include <Jolt/Physics/Collision/Shape/BoxShape.h>
#include <Jolt/Physics/Collision/Shape/MeshShape.h>
#include <Jolt/Physics/Collision/Shape/SphereShape.h>
#include <Jolt/Physics/Collision/ShapeCast.h>
#include <Jolt/Physics/PhysicsSettings.h>
#include <Jolt/Physics/PhysicsSystem.h>
#include <Jolt/RegisterTypes.h>

enum PhysicsType
{
	PHYSICS_MODE_STATIC,
	PHYSICS_MODE_DYNAMIC
};

enum PhysicsShapeType
{
	PHYSICS_SHAPE_BOX,
	PHYSICS_SHAPE_SPHERE,
	PHYSICS_SHAPE_MESH
};

struct PhysicsShapeData
{
	float radius;
	Vector3 extents;

	std::vector<Vector3> vertices;
};

struct PhysicsShape
{
	PhysicsShapeType shapeType;
	PhysicsShapeData shapeData;
};

struct PhysicsBody
{
	PhysicsType type;
	PhysicsShape shape;

	Transform transform;
	float restitution;
	float friction;

	bool ignoreRotation;
	bool ignorePosition;

	JPH::BodyID bodyId;
};

struct TraceResult
{
	bool hit;
	Vector3 startPosition;
	Vector3 endPosition;
	float fraction;
	Vector3 normal;
	bool startedSolid;
	bool endedSolid;
	uint32_t pad0;
	uint32_t entityHandle;

	static TraceResult Empty( Vector3 _startPosition, Vector3 _endPosition )
	{
		TraceResult result = { false, _startPosition, _endPosition, 1.0f, -1, false, false };

		result.entityHandle = UINT32_MAX;

		return result;
	}
};

struct TraceInfo
{
	Vector3 startPosition;
	Vector3 endPosition;

	bool isBox;
	Vector3 extents;

	int ignoredEntityCount;
	uint32_t* ignoredEntityHandles;
};

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

namespace JoltConversions
{
	// Convert Mocha to Jolt Vector3.
	inline JPH::Vec3 MochaToJoltVec3( Vector3 inVec3 )
	{
		return JPH::Vec3{ inVec3.x, inVec3.y, inVec3.z };
	}

	// Convert Mocha Vector3 to Jolt Float3.
	inline JPH::Float3 MochaToJoltFloat3( Vector3 inVec3 )
	{
		return JPH::Float3{ inVec3.x, inVec3.y, inVec3.z };
	}

	// Convert Jolt to Mocha Vector3.
	inline Vector3 JoltToMochaVec3( JPH::Vec3 inVec3 )
	{
		return Vector3{ inVec3.GetX(), inVec3.GetY(), inVec3.GetZ() };
	}

	// Convert Jolt to Mocha Quaternion
	inline Quaternion JoltToMochaQuat( JPH::Quat inQuat )
	{
		return Quaternion{ inQuat.GetX(), inQuat.GetY(), inQuat.GetZ(), inQuat.GetW() };
	}

	// Convert Mocha to Jolt Quaternion
	inline JPH::Quat MochaToJoltQuat( Quaternion inQuat )
	{
		JPH::Quat q = JPH::Quat{ inQuat.x, inQuat.y, inQuat.z, inQuat.w };
		return q.Normalized();
	}
}; // namespace JoltConversions

inline static std::atomic<bool> AreTypesRegistered = false;

class PhysicsManager : HandleMap<PhysicsBody>, ISubSystem
{
private:
	struct PhysicsInstance
	{
		JPH::TempAllocator* m_tempAllocator;
		JPH::JobSystem* m_jobSystem;
		JPH::PhysicsSystem m_physicsSystem;
		BPLayerInterfaceImpl m_broadPhaseLayerInterface;
	};

	std::shared_ptr<PhysicsInstance> m_physicsInstance;

	uint32_t FindEntityHandleForBodyId( JPH::BodyID bodyId );

	TraceResult TraceRay( TraceInfo traceInfo );
	TraceResult TraceBox( TraceInfo traceInfo );
	bool IsBodyIgnored( TraceInfo& traceInfo, JPH::BodyID bodyId );

public:
	PhysicsManager();

	void Startup() override;
	void Shutdown() override;

	void Update();

	uint32_t AddBody( ModelEntity* entity, PhysicsBody body );
	GENERATE_BINDINGS TraceResult Trace( TraceInfo traceInfo );
};
