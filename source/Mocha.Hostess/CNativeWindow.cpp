#include "CNativeWindow.h"

#include "CEngine.h"
#include "CImgui.h"
#include "Globals.h"
#include "imgui_impl_sdl.h"

#include <SDL2/SDL_image.h>
#include <SDL2/sdl_syswm.h>
#include <dwmapi.h>
#define DWMWA_USE_IMMERSIVE_DARK_MODE 20

#include <spdlog/spdlog.h>

CNativeWindow::CNativeWindow( std::string title, int width, int height )
{
	mSdlWindow =
	    SDL_CreateWindow( title.c_str(), SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, SDL_WINDOW_SHOWN );
	SDL_Surface* icon = IMG_Load( "..\\content\\logo.ico" );
	SDL_SetWindowIcon( mSdlWindow, icon );
	SDL_SetWindowResizable( mSdlWindow, SDL_TRUE );

	HWND hwnd = GetWindowHandle();
	BOOL value = TRUE;
	DwmSetWindowAttribute( hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, &value, sizeof( value ) );
}

void CNativeWindow::Run( std::function<void()> renderFunction )
{
	while ( g_EngineIsRunning )
	{
		SDL_Event event;

		while ( SDL_PollEvent( &event ) )
		{
			ImGui_ImplSDL2_ProcessEvent( &event );

			if ( event.type == SDL_QUIT )
			{
				return;
			}
			else if ( event.type == SDL_WINDOWEVENT )
			{
				// Handle resizing here
				if ( event.window.event == SDL_WINDOWEVENT_RESIZED )
				{
					SDL_WindowEvent windowEvent = event.window;
					Uint2 newSize = { windowEvent.data1, windowEvent.data2 };

					g_Engine->GetRenderer()->Resize( newSize );
				}
			}
		}

		renderFunction();
	}
}

SDL_Window* CNativeWindow::GetWindowPointer()
{
	return mSdlWindow;
}

HWND CNativeWindow::GetWindowHandle()
{
	SDL_SysWMinfo wmInfo;
	SDL_VERSION( &wmInfo.version );
	SDL_GetWindowWMInfo( mSdlWindow, &wmInfo );
	HWND hwnd = wmInfo.info.win.window;

	return hwnd;
}

Uint2 CNativeWindow::GetWindowSize()
{
	return mWindowSize;
}
