#pragma once
#include "types.h"

#include <memory>

//@InteropGen generate class
class Camera
{
public:
	Transform m_transform = {};

	Camera();
	void SetPosition( Vector3 position );
	void Update( int m_frameNumber );

	//@InteropGen ignore
	glm::mat4 GetProjectionViewMatrix();
};