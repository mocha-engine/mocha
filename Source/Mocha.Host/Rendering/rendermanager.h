#pragma once

#include <Misc/defs.h>
#include <Misc/subsystem.h>
#include <Rendering/baserendercontext.h>
#include <Rendering/window.h>
#include <functional>
#include <glm/glm.hpp>
#include <imgui.h>
#include <vector>

class ModelEntity;

class RenderManager : ISubSystem
{
private:
	std::unique_ptr<BaseRenderContext> m_renderContext;

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

	void RenderEntity( ModelEntity* entity );

	// Render a mesh. This will handle all the pipelines, descriptors, buffers, etc. for you - just call
	// this once and it'll do all the work.
	// Note that this will render to whatever render target is currently bound (see BindRenderTarget).
	void RenderMesh( RenderPushConstants constants, Mesh* mesh );

public:
	void Startup();
	void Shutdown();

	void DrawOverlaysAndEditor();
	void DrawGame();

	const char* GetGPUName()
	{
		GPUInfo info{};
		assert( m_renderContext->GetGPUInfo( &info ) == RENDER_STATUS_OK );
		return info.gpuName;
	}

	Size2D GetWindowExtent()
	{
		Size2D size{};
		assert( m_renderContext->GetRenderSize( &size ) == RENDER_STATUS_OK );
		return size;
	}
};