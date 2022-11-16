#pragma once
#include <SDL2/SDL.h>
#include <SDL2/SDL_vulkan.h>

#include <spdlog/spdlog.h>

class CWindow
{
private:
	struct SDL_Window* m_window{ nullptr };

public:
	CWindow( uint32_t width, uint32_t height );

	VkSurfaceKHR CreateSurface( VkInstance instance );
	void Cleanup();
	bool Update();
};
