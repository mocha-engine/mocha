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
class Material;

class RenderPass
{
public:
	virtual void Execute() = 0;
	virtual ~RenderPass() = default;

	void SetInputTexture( RenderTexture texture );
	void SetOutputTexture( RenderTexture texture );
};

class SceneMeshPass : public RenderPass
{
public:
	void Execute() override;
	void AddMesh( std::shared_ptr<SceneMesh> sceneMesh );
	void SetConstants( std::shared_ptr<RenderPushConstants> constants );

private:
	std::shared_ptr<RenderPushConstants> m_constants;
	std::vector<std::shared_ptr<SceneMesh>> m_meshes;

	glm::mat4x4 CalculateViewProjMatrix();
	glm::mat4x4 CalculateViewmodelViewProjMatrix();

	void RenderSceneMesh( SceneMesh* mesh );

	// Render a mesh. This will handle all the pipelines, descriptors, buffers, etc. for you - just call
	// this once and it'll do all the work.
	// Note that this will render to whatever render target is currently bound (see BindRenderTarget).
	void RenderMesh( RenderPushConstants constants, Mesh* mesh );
};

class TonemapPass : public RenderPass
{
public:
	explicit TonemapPass( std::shared_ptr<Material> material );
	void Execute() override;

private:
	std::shared_ptr<Material> m_material;
};

class RenderManager : ISubSystem
{
private:
	std::unique_ptr<BaseRenderContext> m_renderContext{};

public:
	void Startup();
	void Shutdown();
	
	void Render();
};