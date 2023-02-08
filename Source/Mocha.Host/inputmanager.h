#pragma once
#include <cstdint>
#include <defs.h>
#include <mathtypes.h>
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
	InputManager( Root* parent )
	    : ISubSystem( parent )
	{
	}

	void Startup() override{};
	void Shutdown() override{};

	InputState GetState();
	void SetState( InputState newState );

	GENERATE_BINDINGS bool IsButtonDown( int button );
	GENERATE_BINDINGS bool IsKeyDown( int key );
	GENERATE_BINDINGS Vector2 GetMousePosition();
	GENERATE_BINDINGS Vector2 GetMouseDelta();
};
