#include "inputmanager.h"


void InputManager::Startup() {}

void InputManager::Shutdown() {}

InputState InputManager::GetState()
{
	return m_inputState;
}

void InputManager::SetState( InputState newState )
{
	m_inputState = newState;
}

bool InputManager::IsButtonDown( int button )
{
	if ( m_inputState.buttons.size() <= button )
		m_inputState.buttons.resize( button + 1 );

	return m_inputState.buttons[button];
}

bool InputManager::IsKeyDown( int key )
{
	if ( m_inputState.keys.size() <= key )
		m_inputState.keys.resize( key + 1 );

	return m_inputState.keys[key];
}

Vector2 InputManager::GetMousePosition()
{
	return m_inputState.mousePosition;
}

Vector2 InputManager::GetMouseDelta()
{
	return m_inputState.mouseDelta;
}