#include "window.h"

#include <defs.h>

#ifdef _IMGUI
#include <thirdparty/imgui/imgui.h>
#include <thirdparty/imgui/imgui_impl_sdl.h>
#endif

Window::Window( uint32_t width, uint32_t height )
{
	SDL_Init( SDL_INIT_VIDEO );

	SDL_WindowFlags windowFlags = ( SDL_WindowFlags )( SDL_WINDOW_VULKAN | SDL_WINDOW_RESIZABLE );

	m_window = SDL_CreateWindow( GAME_NAME, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, windowFlags );
}

VkSurfaceKHR Window::CreateSurface( VkInstance instance )
{
	VkSurfaceKHR surface;
	SDL_Vulkan_CreateSurface( m_window, instance, &surface );
	return surface;
}

void Window::Cleanup()
{
	SDL_DestroyWindow( m_window );
}

bool Window::Update()
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

#ifdef _IMGUI
		// Pipe event to imgui too
		ImGui_ImplSDL2_ProcessEvent( &e );
#endif
	}

	return false;
}