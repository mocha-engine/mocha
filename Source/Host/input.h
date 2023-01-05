#pragma once
#include <cvarmanager.h>
#include <globalvars.h>
#include <inputmanager.h>
#include <defs.h>

namespace Input
{
	GENERATE_BINDINGS inline bool IsButtonDown( int button )
	{
		return g_inputManager->IsButtonDown( button );
	}

	GENERATE_BINDINGS inline Vector2 GetMousePosition()
	{
		return g_inputManager->GetMousePosition();
	}

	GENERATE_BINDINGS inline Vector2 GetMouseDelta()
	{
		return g_inputManager->GetMouseDelta();
	}

	GENERATE_BINDINGS inline bool IsKeyDown( int key )
	{
		return g_inputManager->IsKeyDown( key );
	}
} // namespace Input