#pragma once
#include <SDL2/SDL.h>

//@InteropGen generate class
class CNativeWindow
{
private:
	SDL_Window* sdl_window;

public:
	void Create(const char* title, int width, int height);
	void Run();

	SDL_Window* GetWindowPointer();
};