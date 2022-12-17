#pragma once
#include <cstdint>
#include <game_types.h>
#include <memory.h>
#include <subsystem.h>
#include <vector>

struct InputState
{
	std::vector<bool> buttons;
	std::vector<bool> keys;

	Vector2 mousePosition;
	Vector2 lastMousePosition;
	Vector2 mouseDelta;
};

class InputManager : ISubSystem
{
private:
	InputState m_inputState;

public:
	void Startup() override;
	void Shutdown() override;

	InputState GetState();
	void SetState( InputState newState );

	bool IsButtonDown( int button );
	bool IsKeyDown( int key );
	Vector2 GetMousePosition();
	Vector2 GetMouseDelta();
};
