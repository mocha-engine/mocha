#pragma once
#include <game/camera.h>
#include <game/types.h>
#include <stdint.h>
#include <string>
#include <vulkan/vulkan.h>

class BaseEntity
{
private:
	std::string m_name;
	Transform m_transform;

public:
	inline BaseEntity() { m_name = "Unnamed Entity"; }
	virtual ~BaseEntity() {}

	virtual void Render( VkCommandBuffer cmd, Camera* camera ){};

	inline void SetName( std::string name ) { m_name = name; }
	inline const char* GetName() { return m_name.c_str(); }

	inline Transform GetTransform() { return m_transform; }
	inline void SetTransform( Transform transform ) { m_transform = transform; }
};
