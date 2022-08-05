#include "CNativeWindow.h"
#include <spdlog/spdlog.h>
#include <SDL2/SDL_image.h>

#include <SDL2/sdl_syswm.h>
#include <dwmapi.h>
#define DWMWA_USE_IMMERSIVE_DARK_MODE 20

CNativeWindow::CNativeWindow(std::string title, int width, int height)
{
	m_SdlWindow = SDL_CreateWindow(title.c_str(), SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, SDL_WINDOW_SHOWN);
	SDL_Surface* icon = IMG_Load("..\\content\\logo.ico");
	SDL_SetWindowIcon(m_SdlWindow, icon);
	SDL_SetWindowResizable(m_SdlWindow, SDL_TRUE);

	HWND hwnd = GetWindowHandle();
	BOOL value = TRUE;

	DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &value, sizeof(value));
	m_Renderer = std::make_unique<CRenderer>(this);
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
			else if (event.type == SDL_WINDOWEVENT)
			{
				if (event.window.event == SDL_WINDOWEVENT_RESIZED)
				{
					SDL_WindowEvent windowEvent = event.window;
					Uint2 newSize = { windowEvent.data1, windowEvent.data2 };
					m_Renderer->Resize(newSize);
				}
			}
		}

		m_Renderer->Render();
	}

	m_Renderer = nullptr;
}

SDL_Window* CNativeWindow::GetWindowPointer()
{
	return m_SdlWindow;
}

HWND CNativeWindow::GetWindowHandle()
{
	SDL_SysWMinfo wmInfo;
	SDL_VERSION(&wmInfo.version);
	SDL_GetWindowWMInfo(m_SdlWindow, &wmInfo);
	HWND hwnd = wmInfo.info.win.window;

	return hwnd;
}

Uint2 CNativeWindow::GetWindowSize()
{
	return m_WindowSize;
}
