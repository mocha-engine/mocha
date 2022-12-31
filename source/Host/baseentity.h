#pragma once
#include <game_types.h>
#include <stdint.h>
#include <string>
#include <volk.h>

enum EntityFlags : int
{
	ENTITY_MANAGED = 1 << 0,
	ENTITY_RENDERABLE = 1 << 1,
	ENTITY_VIEWMODEL = 1 << 2,
	ENTITY_UI = 1 << 3,
};

class Camera;

class BaseEntity
{
protected:
	std::string m_type;
	std::string m_name;

	Transform m_transform;

	int m_flags;

public:
	BaseEntity();
	virtual ~BaseEntity() {}

	//
	// Getters & setters
	//
	inline void SetName( std::string name ) { m_name = name; }
	inline const char* GetName() { return m_name.c_str(); }

	inline Transform GetTransform() { return m_transform; }
	inline void SetTransform( Transform transform ) { m_transform = transform; }

	inline EntityFlags GetFlags() { return ( EntityFlags )m_flags; }
	inline void SetFlags( EntityFlags flags ) { m_flags = flags; }
	inline void AddFlag( EntityFlags flags ) { m_flags = m_flags | flags; }
	inline void RemoveFlag( EntityFlags flags ) { m_flags = m_flags & ~flags; }
	inline bool HasFlag( EntityFlags flag ) { return ( m_flags & flag ) != 0; }

	inline void SetType( std::string type ) { m_type = type; }
	inline const char* GetType() { return m_type.c_str(); }
};
