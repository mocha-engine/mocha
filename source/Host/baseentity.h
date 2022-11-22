#pragma once
#include <game/camera.h>
#include <game/types.h>
#include <stdint.h>
#include <string>
#include <vulkan/vulkan.h>

enum EntityFlags : int
{
	ENTITY_MANAGED = 1 << 0,
	ENTITY_RENDERABLE = 1 << 1
};

class BaseEntity
{
private:
	std::string m_name;
	Transform m_transform;
	int m_flags;

public:
	inline BaseEntity() { m_name = "Unnamed Entity"; }
	virtual ~BaseEntity() {}

	virtual void Render( VkCommandBuffer cmd, Camera* camera ){};

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
	inline bool HasFlag( EntityFlags flag ) { return ( m_flags & flag ) != 0; }
};