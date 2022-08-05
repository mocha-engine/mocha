#include "CNativeWindow.h"
#include <spdlog/spdlog.h>
#include <SDL2/SDL_image.h>

#if WINDOWS
#include <SDL2/sdl_syswm.h>
#include <dwmapi.h>
#define DWMWA_USE_IMMERSIVE_DARK_MODE 20
#endif

CNativeWindow::CNativeWindow(std::string title, int width, int height)
{
	sdl_window = SDL_CreateWindow(title.c_str(), SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, SDL_WINDOW_SHOWN);
	SDL_Surface* icon = IMG_Load("..\\content\\logo.ico");
	SDL_SetWindowIcon(sdl_window, icon);

#if WINDOWS
	SDL_SysWMinfo wmInfo;
	SDL_VERSION(&wmInfo.version);
	SDL_GetWindowWMInfo(sdl_window, &wmInfo);
	HWND hwnd = wmInfo.info.win.window;

	BOOL value = TRUE;

	DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &value, sizeof(value));
#endif
}

void CNativeWindow::Run()
{
	while (true)
	{
		SDL_Event event;
		while (SDL_PollEvent(&event))
		{
			if (event.type == SDL_QUIT)
			{
				return;
			}
		}
	}
}

SDL_Window* CNativeWindow::GetWindowPointer()
{
	return sdl_window;
}