#pragma once

#include <functional>
#include <glm/glm.hpp>
#include <subsystem.h>
#include <vector>
#include <vk_types.h>
#include <window.h>

class Model;
class HostManager;
class Camera;

class RenderManager : ISubSystem
{
private:
	void InitVulkan();
	void InitDeviceProperties();
	void InitSwapchain();
	void InitCommands();
	void InitSyncStructures();
	void InitImGUI();
	void InitDescriptors();
	void InitSamplers();

	void CreateSwapchain( VkExtent2D size );

	void CalculateCameraMatrices( glm::mat4x4& viewMatrix, glm::mat4x4& projMatrix );

public:
	bool m_isInitialized{ false };
	int m_frameNumber{ 0 };

	VkSampler m_pointSampler;
	VkSampler m_anisoSampler;

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

	VkDescriptorPool m_descriptorPool;

	void Startup();
	void Shutdown();

	void Render();
	void Run();

	std::string m_deviceName;

	UploadContext m_uploadContext;
	void ImmediateSubmit( std::function<void( VkCommandBuffer cmd )>&& function );

	AllocatedBuffer CreateBuffer( size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage );

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

	VkExtent2D GetWindowExtent();
};
