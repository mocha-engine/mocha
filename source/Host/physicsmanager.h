#pragma once
#include <unordered_map>
#include <subsystem.h>
#include <game/types.h>
#include <memory>

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

public:
	void Startup() override;
	void Shutdown() override;

	void Update();

	uint32_t AddBody( PhysicsBody body );
	Transform* GetTransform( uint32_t bodyHandle );
};
