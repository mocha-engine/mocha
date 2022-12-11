#pragma once
#include <Jolt/Jolt.h>
#include <game/types.h>
#include <memory>
#include <spdlog/spdlog.h>
#include <subsystem.h>
#include <unordered_map>

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

enum PhysicsType
{
	Static,
	Dynamic
};

enum PhysicsShapeType
{
	Box,
	Sphere
};

struct PhysicsShapeData
{
	float radius;
	Vector3 extents;
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
};

class PhysicsManager : ISubSystem
{
private:
	std::unordered_map<uint32_t, std::shared_ptr<PhysicsBody>> m_bodies;
	uint32_t m_bodyIndex;

	JPH::TempAllocator* m_tempAllocator;
	JPH::JobSystem* m_jobSystem;
	JPH::PhysicsSystem m_physicsSystem;

	JPH::BodyID m_sphereId;

	JPH::uint m_step = 0;

public:
	PhysicsManager();

	static void PreInit();

	void Startup() override;
	void Shutdown() override;

	void Update();

	uint32_t AddBody( PhysicsBody body );
	Transform* GetTransform( uint32_t bodyHandle );
};
