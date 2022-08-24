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

#define SECONDS_TO_NANOSECONDS(x) (x * 1000000000)

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

	//
	// Commands
	//
	VkQueue mGraphicsQueue;
	uint32_t mGraphicsQueueFamily;
	
	VkCommandPool mCommandPool;
	VkCommandBuffer mCommandBuffer;
	
	//
	// Render pass
	//
	VkRenderPass mRenderPass;
	std::vector<VkFramebuffer> mFramebuffers;

	//
	// Main loop / synchronization
	//
	VkSemaphore mPresentSemaphore, mRenderSemaphore;
	VkFence mRenderFence;

public:
	CRenderer( CWindow* window );
	~CRenderer();

	void Render();

	void Resize( Uint2 size );

	void InitAPI();
	void InitSwapchain();
	void InitCommands();
	void InitDefaultRenderPass();
	void InitFramebuffers();
	void InitSyncStructures();

	void Cleanup();
};
