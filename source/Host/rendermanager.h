#pragma once

#include <baserendercontext.h>
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

class RenderManager : ISubSystem
{
private:
	std::unique_ptr<BaseRenderContext> m_renderContext;

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

public:
	void Startup();
	void Shutdown();

	void Render();
	void Run();

	VkExtent2D GetWindowExtent();
	VkExtent2D GetDesktopSize();
};
