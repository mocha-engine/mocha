#pragma once
#include <SDL2/SDL.h>
#include <SDL2/SDL_vulkan.h>
#include <functional>
#include <spdlog/spdlog.h>
#include <vulkan/types.h>

class Window
{
private:
	struct SDL_Window* m_window{ nullptr };

public:
	Window( uint32_t width, uint32_t height );

	std::function<void( VkExtent2D newWindowExtents )> m_onWindowResized;

	VkSurfaceKHR CreateSurface( VkInstance instance );
	void Cleanup();
	bool Update();

	inline SDL_Window* GetSDLWindow() { return m_window; }
	inline void GetWindowSize( int* outW, int* outH ) { SDL_GetWindowSize( m_window, outW, outH ); }
};
