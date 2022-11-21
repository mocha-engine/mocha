#include "camera.h"

#include <glm/ext.hpp>

#include <globalvars.h>
#include <vulkan/rendermanager.h>

Camera::Camera() 
{
}

void Camera::SetPosition( Vector3 position )
{
	g_renderManager->SetCamera( this );
	
	m_transform.position = position;
}

void Camera::Update( int m_frameNumber )
{
	// m_transform.position.x = sin( m_frameNumber * 3 * 0.0025f ) * 4.0f;
	// m_transform.position.y = sin( m_frameNumber * 2 * 0.0025f ) * 4.0f;
	// m_transform.position.z = cos( m_frameNumber * 1 * 0.0025f ) * 4.0f;
}

glm::mat4 Camera::GetProjectionViewMatrix()
{
	glm::vec3 camPos = m_transform.position.ToGLM();
	glm::mat4 view = glm::lookAt( camPos, glm::vec3( 0, 0, 0 ), glm::vec3( 0, 1, 0 ) );
	glm::mat4 projection = glm::perspective( glm::radians( 70.f ), 16.0f / 9.0f, 0.1f, 200.0f );

	return projection * view;
}
