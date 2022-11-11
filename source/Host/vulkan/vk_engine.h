#pragma once
#include "../game/g_camera.h"
#include "../game/g_model.h"
#include "../game/g_types.h"
#include "../window.h"
#include "vk_types.h"

#include <glm/glm.hpp>
#include <memory>
#include <vector>

class CNativeEngine
{
private:
	void InitVulkan();
	void InitSwapchain();
	void InitCommands();
	void InitSyncStructures();

public:
	bool m_isInitialized{ false };
	int m_frameNumber{ 0 };

	VkExtent2D m_windowExtent{ 1280, 720 };
	std::unique_ptr<CWindow> m_window;

	VkInstance m_instance;
	VkDebugUtilsMessengerEXT m_debugMessenger;
	VkPhysicalDevice m_chosenGPU;
	VkDevice m_device;
	VkSurfaceKHR m_surface;

	VkSwapchainKHR m_swapchain;
	VkFormat m_swapchainImageFormat;

	VkQueue m_graphicsQueue;
	uint32_t m_graphicsQueueFamily;

	VkCommandPool m_commandPool;
	VkCommandBuffer m_commandBuffer;

	std::vector<VkImage> m_swapchainImages;
	std::vector<VkImageView> m_swapchainImageViews;

	VkSemaphore m_presentSemaphore, m_renderSemaphore;
	VkFence m_renderFence;

	VmaAllocator m_allocator;

	Model m_triangle;
	Camera m_camera;

	void Init();
	void Cleanup();
	void Render();
	void Run();
};
