#pragma once
#include <SDL2/SDL.h>
#include <SDL2/SDL_vulkan.h>
#include <functional>
#include <mathtypes.h>
#include <spdlog/spdlog.h>

class Window
{
private:
	struct SDL_Window* m_window{ nullptr };
	bool m_captureMouse;

public:
	Window( uint32_t width, uint32_t height );

	std::function<void( Size2D newSize )> m_onWindowResized;

	VkSurfaceKHR CreateSurface( VkInstance instance );
	void Cleanup();
	bool Update();

	inline SDL_Window* GetSDLWindow() { return m_window; }

	inline Size2D GetWindowSize()
	{
		int outW, outH;

		if ( ( SDL_GetWindowFlags( m_window ) & SDL_WINDOW_MINIMIZED ) == 0 )
		{
			SDL_GetWindowSize( m_window, &outW, &outH );
		}
		else
		{
			outW = 0;
			outH = 0;
		}

		return { ( uint32_t )outW, ( uint32_t )outH };
	}

	inline void GetDesktopSize( int* outW, int* outH )
	{
		SDL_DisplayMode dm;
		SDL_GetCurrentDisplayMode( 0, &dm );
		*outW = dm.w;
		*outH = dm.h;
	}
};
