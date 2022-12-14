#pragma once
#include <cvarmanager.h>
#include <globalvars.h>
#include <inputmanager.h>

//@InteropGen generate class
namespace Input
{
	inline bool IsButtonDown( int button )
	{
		return g_inputManager->IsButtonDown( button );
	}

	inline Vector2 GetMousePosition()
	{
		return g_inputManager->GetMousePosition();
	}

	inline Vector2 GetMouseDelta()
	{
		return g_inputManager->GetMouseDelta();
	}

	inline bool IsKeyDown( int key )
	{
		return g_inputManager->IsKeyDown( key );
	}
} // namespace Input