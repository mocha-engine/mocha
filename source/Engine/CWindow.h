#pragma once
#include "CNetCoreHost.h"
#include "CRenderer.h"
#include "Uint2.h"

#include <SDL2/SDL.h>
#include <string>

class CWindow
{
private:
	SDL_Window* mSdlWindow;
	Uint2 mWindowSize;

public:
	CWindow( std::string title, int width, int height );

	void Run( std::function<void()> renderFunction );

	SDL_Window* GetWindowPointer();
	Uint2 GetWindowSize();
	HWND GetWindowHandle();
};