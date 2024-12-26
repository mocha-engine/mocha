#pragma once
#include <Misc/defs.h>
#include <Misc/mathtypes.h>
#include <Misc/subsystem.h>
#include <cstdint>
#include <memory.h>
#include <vector>

struct InputState
{
	std::vector<bool> buttons;
	std::vector<bool> keys;

	Vector2 mousePosition;
	Vector2 lastMousePosition;
	Vector2 mouseDelta;

	bool isMouseCaptured;
};

class InputManager : ISubSystem
{
private:
	InputState m_inputState;

public:
	void Startup() override{};
	void Shutdown() override{};

	InputState GetState();
	void SetState( InputState newState );

	GENERATE_BINDINGS bool IsButtonDown( int button );
	GENERATE_BINDINGS bool IsKeyDown( int key );
	GENERATE_BINDINGS Vector2 GetMousePosition();
	GENERATE_BINDINGS Vector2 GetMouseDelta();
	GENERATE_BINDINGS bool IsMouseCaptured();
};
