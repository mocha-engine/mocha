#pragma once
#define GLM_FORCE_SSE42 1
#define GLM_FORCE_DEFAULT_ALIGNED_GENTYPES 1
#define GLM_FORCE_LEFT_HANDED
#include <glm/glm.hpp>
using namespace glm;

#include "CImgui.h"
#include "Uint2.h"
#include "VkBootstrap.h"

#include <SDL2/SDL_vulkan.h>
#include <functional>
#include <vector>

typedef void ( *render_callback_fn )( ID3D12GraphicsCommandList* );

#include <memory>

class CWindow;

class CRenderer
{
private:
	unsigned mWidth, mHeight;
	CWindow* mWindow;

	//
	// Vk boilerplate
	//
	VkInstance mInstance;
	VkDebugUtilsMessengerEXT mDebugMessenger;
	VkPhysicalDevice mPhysicalDevice;
	VkDevice mDevice;
	VkSurfaceKHR mSurface;

	//
	// Swapchain
	//
	VkSwapchainKHR mSwapchain;
	VkFormat mSwapchainImageFormat;
	std::vector<VkImage> mSwapchainImages;
	std::vector<VkImageView> mSwapchainImageViews;

public:
	CRenderer( CWindow* window );
	~CRenderer();

	void BeginFrame();
	void EndFrame();

	void Resize( Uint2 size );

	void InitAPI();
	void InitSwapchain();

	void Cleanup();
};
