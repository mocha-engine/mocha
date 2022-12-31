#pragma once

#include <defs.h>
#include <functional>
#include <glm/glm.hpp>
#include <imgui.h>
#include <subsystem.h>
#include <vector>
#include <vk_types.h>
#include <window.h>

struct Mesh;
class Model;
class HostManager;
class Camera;

struct BlasInput
{
public:
	std::vector<VkAccelerationStructureGeometryKHR> asGeometry = {};
	std::vector<VkAccelerationStructureBuildRangeInfoKHR> asBuildOffsetInfo = {};
	VkBuildAccelerationStructureFlagsKHR flags;
};

struct AllocatedAccel
{
	VkAccelerationStructureKHR accel;
	AllocatedBuffer buffer;
};

struct BuildAccelerationStructure
{
	VkAccelerationStructureBuildGeometryInfoKHR buildInfo = {};
	VkAccelerationStructureBuildSizesInfoKHR sizeInfo = {};
	VkAccelerationStructureBuildRangeInfoKHR* rangeInfo = {};

	AllocatedAccel as;
	AllocatedAccel cleanupAS;
};

class RenderManager : ISubSystem
{
private:

	void InitRayTracing();
	VkPhysicalDeviceRayTracingPipelinePropertiesKHR m_rtProperties;

	BlasInput ModelToVkGeometry( Model& model );
	void CreateBottomLevelAS();
	void CreateTopLevelAS();

	VkDeviceAddress GetBlasDeviceAddress( uint32_t handle );

	AllocatedAccel CreateAcceleration( VkAccelerationStructureCreateInfoKHR& createInfo );

	void CmdCreateBlas( VkCommandBuffer cmdBuf, std::vector<uint32_t> indices, std::vector<BuildAccelerationStructure>& buildAs,
	    VkDeviceAddress scratchAddress, VkQueryPool queryPool );

	void CmdCompactBlas( VkCommandBuffer cmdBuf, std::vector<uint32_t> indices,
	    std::vector<BuildAccelerationStructure>& buildAs, VkQueryPool queryPool );

	void CmdCreateTlas( VkCommandBuffer cmdBuf, uint32_t countInstance, VkDeviceAddress instBufferAddr,
	    AllocatedBuffer& scratchBuffer, VkBuildAccelerationStructureFlagsKHR flags, bool update );

	std::vector<AllocatedAccel> m_blas = {};

	uint32_t GetMemoryType( uint32_t typeBits, VkMemoryPropertyFlags properties, VkBool32* memTypeFound = nullptr ) const;

	VkPhysicalDeviceMemoryProperties m_memoryProperties;

	void CalculateCameraMatrices( glm::mat4x4& viewMatrix, glm::mat4x4& projMatrix );

public:
	bool m_isInitialized{ false };
	int m_frameNumber{ 0 };

	VkSampler m_pointSampler;
	VkSampler m_anisoSampler;

	VkImageView m_depthImageView;
	AllocatedImage m_depthImage;

	VkFormat m_depthFormat;

	VkFence m_renderFence;

	VkDescriptorPool m_descriptorPool;

	void Startup();
	void Shutdown();

	void Render();
	void Run();

	std::string m_deviceName;

	UploadContext m_uploadContext;
	void ImmediateSubmit( std::function<void( VkCommandBuffer cmd )>&& function );

	AllocatedBuffer CreateBuffer( size_t allocationSize, VkBufferUsageFlags usage, VmaMemoryUsage memoryUsage,
	    VmaAllocationCreateFlagBits allocFlags = VMA_ALLOCATION_CREATE_HOST_ACCESS_SEQUENTIAL_WRITE_BIT );

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

	VkExtent2D GetWindowExtent();
	VkExtent2D GetDesktopSize();

	ImFont* m_mainFont;
	ImFont* m_monospaceFont;

	AllocatedAccel m_tlas = {};
};
