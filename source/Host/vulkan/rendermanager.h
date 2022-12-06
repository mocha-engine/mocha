#pragma once

#include <glm/glm.hpp>
#include <vector>
#include <functional>
#include <subsystem.h>

// Vulkan types
#include "types.h"
#include "../window.h"

class Model;
class HostManager;
class Camera;

class RenderManager : ISubSystem
{
private:
	void InitVulkan();
	void InitSwapchain();
	void InitCommands();
	void InitSyncStructures();
	void InitImGUI();

public:
	bool m_isInitialized{ false };
	int m_frameNumber{ 0 };

	VkExtent2D m_windowExtent{ 1280, 720 };
	std::unique_ptr<Window> m_window;

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

	VkImageView m_depthImageView;
	AllocatedImage m_depthImage;

	VkFormat m_depthFormat;

	VkSemaphore m_presentSemaphore, m_renderSemaphore;
	VkFence m_renderFence;

	VmaAllocator m_allocator;

	void Startup();
	void Shutdown();
	
	void Render();
	void Run();

	UploadContext m_uploadContext;
	void ImmediateSubmit( std::function<void ( VkCommandBuffer cmd )>&& function );

	glm::mat4x4 CalculateViewProjMatrix();
};
