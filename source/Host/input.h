#pragma once
#include <globalvars.h>
#include <inputmanager.h>
#include <cvarmanager.h>

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
} // namespace Input