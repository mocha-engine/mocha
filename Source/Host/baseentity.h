#pragma once
#include <defs.h>
#include <mathtypes.h>
#include <stdint.h>
#include <string>

enum EntityFlags : int
{
	ENTITY_NONE = 1 << 0,
	ENTITY_MANAGED = 1 << 1,
	ENTITY_RENDERABLE = 1 << 2,
	ENTITY_VIEWMODEL = 1 << 3,
	ENTITY_UI = 1 << 4,
};

DEFINE_FLAG_OPERATORS( EntityFlags );

class Camera;

class BaseEntity
{
public:
	BaseEntity(){};
	virtual ~BaseEntity() {}

	EntityFlags m_flags = ENTITY_NONE;

	std::string m_type = "No type";
	std::string m_name = "Unnamed";

	Transform m_transformLastFrame = {};
	Transform m_transformCurrentFrame = {};

	Transform m_transform = {};

	inline void AddFlag( EntityFlags flags ) { m_flags = m_flags | flags; }
	inline void RemoveFlag( EntityFlags flags ) { m_flags = m_flags & ~flags; }
	inline bool HasFlag( EntityFlags flag ) { return ( m_flags & flag ) != 0; }
};
