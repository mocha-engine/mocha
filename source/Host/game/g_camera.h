#pragma once
#include "g_types.h"

#include <memory>

//@InteropGen generate class
class Camera
{
public:
	Transform m_transform = {};

	void Update( int m_frameNumber );
	glm::mat4 GetProjectionViewMatrix();
};