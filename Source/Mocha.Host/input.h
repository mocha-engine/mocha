#pragma once
#include <cvarmanager.h>
#include <clientroot.h>
#include <inputmanager.h>
#include <defs.h>

namespace Input
{
	GENERATE_BINDINGS inline bool IsButtonDown( int button )
	{
		auto& root = ClientRoot::GetInstance();
		return root.m_inputManager->IsButtonDown( button );
	}

	GENERATE_BINDINGS inline Vector2 GetMousePosition()
	{
		auto& root = ClientRoot::GetInstance();
		return root.m_inputManager->GetMousePosition();
	}

	GENERATE_BINDINGS inline Vector2 GetMouseDelta()
	{
		auto& root = ClientRoot::GetInstance();
		return root.m_inputManager->GetMouseDelta();
	}

	GENERATE_BINDINGS inline bool IsKeyDown( int key )
	{
		auto& root = ClientRoot::GetInstance();
		return root.m_inputManager->IsKeyDown( key );
	}
} // namespace Input