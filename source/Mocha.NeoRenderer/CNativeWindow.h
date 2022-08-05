#pragma once
#include <string>
#include <SDL2/SDL.h>

//@InteropGen generate class
class CNativeWindow
{
private:
	SDL_Window* sdl_window;

public:
	CNativeWindow(std::string title, int width, int height);

	void Run();
	SDL_Window* GetWindowPointer();
};