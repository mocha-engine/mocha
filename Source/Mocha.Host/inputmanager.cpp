#include <defs.h>
#include "inputmanager.h"

#if _IMGUI

#include <imgui.h>
#define WANTS_CAPTURE IS_CLIENT && ( ImGui::GetIO().WantCaptureKeyboard || ImGui::GetIO().WantCaptureMouse )

#else

#define WANTS_CAPTURE false

#endif

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
	if ( WANTS_CAPTURE )
		return false;

	if ( m_inputState.buttons.size() <= button )
		m_inputState.buttons.resize( button + 1 );

	return m_inputState.buttons[button];
}

bool InputManager::IsKeyDown( int key )
{
	if ( WANTS_CAPTURE )
		return false;

	if ( m_inputState.keys.size() <= key )
		m_inputState.keys.resize( key + 1 );

	return m_inputState.keys[key];
}

Vector2 InputManager::GetMousePosition()
{
	if ( WANTS_CAPTURE )
		return { 0, 0 };

	return m_inputState.mousePosition;
}

Vector2 InputManager::GetMouseDelta()
{
	if ( WANTS_CAPTURE )
		return { 0, 0 };

	return m_inputState.mouseDelta;
}