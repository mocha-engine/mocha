#pragma once
#include <clientroot.h>
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
	BaseEntity()
	    : m_spawnTime( ClientRoot::GetInstance().m_curTick ){};
	virtual ~BaseEntity() {}

	int m_spawnTime;

	EntityFlags m_flags = ENTITY_NONE;

	std::string m_type = "No type";
	std::string m_name = "Unnamed";

	Transform m_transformLastFrame = {};
	Transform m_transformCurrentFrame = {};

	Transform m_transform = {};

	inline void AddFlag( EntityFlags flags ) { m_flags = m_flags | flags; }
	inline void RemoveFlag( EntityFlags flags ) { m_flags = m_flags & ~flags; }
	inline bool HasFlag( EntityFlags flag ) { return ( m_flags & flag ) != 0; }

	//
	// Managed bindings
	//
	GENERATE_BINDINGS inline void SetName( const char* name ) { m_name = name; }
	GENERATE_BINDINGS inline const char* GetName() { return m_name.c_str(); }

	GENERATE_BINDINGS inline void SetType( const char* type ) { m_type = type; }
	GENERATE_BINDINGS inline const char* GetType() { return m_type.c_str(); }

	GENERATE_BINDINGS inline void SetPosition( const Vector3& pos ) { m_transform.position = pos; }
	GENERATE_BINDINGS inline Vector3 GetPosition() { return m_transform.position; }

	GENERATE_BINDINGS inline void SetRotation( const Quaternion& rot ) { m_transform.rotation = rot; }
	GENERATE_BINDINGS inline Quaternion GetRotation() { return m_transform.rotation; }
	
	GENERATE_BINDINGS inline void SetScale( const Vector3& scale ) { m_transform.scale = scale; }
	GENERATE_BINDINGS inline Vector3 GetScale() { return m_transform.scale; }
};
