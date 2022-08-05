#pragma once
#include <string>
#include <SDL2/SDL.h>
#include "CRenderer.h"
#include "Uint2.h"

//@InteropGen generate class
class CNativeWindow
{
private:
	SDL_Window* m_SdlWindow;
	Uint2 m_WindowSize;

	std::unique_ptr<CRenderer> m_Renderer;

public:
	CNativeWindow(std::string title, int width, int height);

	void Run();
	SDL_Window* GetWindowPointer();
	HWND GetWindowHandle();
	Uint2 GetWindowSize();
};