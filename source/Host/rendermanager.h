#pragma once

#include <baserendercontext.h>
#include <defs.h>
#include <functional>
#include <glm/glm.hpp>
#include <imgui.h>
#include <subsystem.h>
#include <vector>
#include <window.h>

class ModelEntity;

class RenderManager : ISubSystem
{
private:
	std::unique_ptr<BaseRenderContext> m_renderContext;

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

	void RenderEntity( ModelEntity* entity );

public:
	void Startup();
	void Shutdown();

	void Render();
	void Run();

	Size2D GetWindowExtent()
	{
		Size2D size;
		assert( m_renderContext->GetRenderSize( &size ) == RENDER_STATUS_OK );
		return size;
	}
};
