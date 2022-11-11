#pragma once
#include <SDL2/SDL.h>
#include <SDL2/SDL_vulkan.h>

#include <spdlog/spdlog.h>

class CWindow
{
private:
	struct SDL_Window* m_window{ nullptr };

public:
	inline CWindow( uint32_t width, uint32_t height )
	{
		SDL_Init( SDL_INIT_VIDEO );

		SDL_WindowFlags windowFlags = ( SDL_WindowFlags )( SDL_WINDOW_VULKAN );

		m_window = SDL_CreateWindow(
		    "Untitled C++ Engine", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, windowFlags );
	}

	inline VkSurfaceKHR CreateSurface( VkInstance instance )
	{
		VkSurfaceKHR surface;
		SDL_Vulkan_CreateSurface( m_window, instance, &surface );
		return surface;
	}

	inline void Cleanup() { SDL_DestroyWindow( m_window ); }

	inline bool Update() 
	{
		SDL_Event e;
		bool bQuit = false;

		while ( SDL_PollEvent( &e ) != 0 )
		{
			if ( e.type == SDL_QUIT )
			{
				return true;
			}
			else if ( e.type == SDL_KEYDOWN )
			{
				SDL_KeyboardEvent ke = e.key;
				char c = SDL_GetKeyFromScancode( ke.keysym.scancode );
				spdlog::info( "Key down: {}", c );
			}
		}

		return false;
	}
};
