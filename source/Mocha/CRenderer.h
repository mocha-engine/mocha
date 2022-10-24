#pragma once
#define GLM_FORCE_SSE42 1
#define GLM_FORCE_DEFAULT_ALIGNED_GENTYPES 1
#define GLM_FORCE_LEFT_HANDED
#include <glm/glm.hpp>
using namespace glm;

#include "CImgui.h"
#include "Observer.h"
#include "Uint2.h"
#include "VkBootstrap.h"

#include <SDL2/SDL_vulkan.h>
#include <functional>
#include <memory>
#include <vector>

#define SECONDS_TO_NANOSECONDS( x ) ( x * 1000000000 )

class CWindow;

class CRenderer : public IObserver
{
private:
	Uint2 mSwapchainSize;
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

	//
	// Pipeline
	//
	VkPipeline mMainPipeline;

	void InitAPI();
	void InitSwapchain();
	void InitCommands();
	void InitDefaultRenderPass();
	void InitFramebuffers();
	void InitSyncStructures();
	void Cleanup();

	void Resize( Uint2 newSize );

public:
	CRenderer( CWindow* window );
	~CRenderer();

	void Render();

	// IObserver
	void OnNotify( Event event, void* data );

	inline VkRenderPass GetMainRenderPass() { return mRenderPass; }
};
