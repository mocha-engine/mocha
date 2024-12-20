#include "rendermanager.h"

//
//
//
#include <Entities/baseentity.h>
#include <Entities/entitymanager.h>
#include <Managed/hostmanager.h>
#include <Misc/cvarmanager.h>
#include <Misc/defs.h>
#include <Misc/projectmanager.h>
#include <Physics/physicsmanager.h>
#include <Root/clientroot.h>
#include <fontawesome.h>

//
//
//
#include <Rendering/window.h>
#include <algorithm>
#include <fstream>
#include <iostream>
#include <memory>

//
//
//
#include <glm/ext.hpp>
#include <spdlog/spdlog.h>

//
//
//
#include <Rendering/Platform/Null/nullrendercontext.h>
#include <Rendering/Platform/Vulkan/vulkanrendercontext.h>

//
//
//
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <imgui.h>
#include <implot.h>

FloatCVar maxFramerate(
    "render.max_framerate", 320.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

const char* GetGPUName()
{
	GPUInfo info{};
	assert( Globals::m_renderContext->GetGPUInfo( &info ) == RENDER_STATUS_OK );
	return info.gpuName;
}

Size2D GetWindowExtent()
{
	Size2D size{};
	assert( Globals::m_renderContext->GetRenderSize( &size ) == RENDER_STATUS_OK );
	return size;
}

glm::mat4 CalculateViewmodelViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( Globals::m_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = Globals::m_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( 60.0f ), aspect, Globals::m_cameraZNear, Globals::m_cameraZFar );

	return projMatrix * viewMatrix;
}

glm::mat4 CalculateViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( Globals::m_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = Globals::m_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix =
	    glm::perspective( glm::radians( Globals::m_cameraFov ), aspect, Globals::m_cameraZNear, Globals::m_cameraZFar );

	return projMatrix * viewMatrix;
}

void RenderManager::Startup()
{
	Globals::m_renderManager = this;

	if ( Globals::m_executingRealm == REALM_CLIENT )
	{
		// Client uses Vulkan for rendering
		m_renderContext = std::make_unique<VulkanRenderContext>();
	}
	else
	{
		// Server is headless - use a null render context
		m_renderContext = std::make_unique<NullRenderContext>();
	}

	Globals::m_renderContext = m_renderContext.get();

	m_renderContext->Startup();
}

void RenderManager::Shutdown()
{
	m_renderContext->Shutdown();
}

void SceneMeshPass::Execute()
{
	// Make a copy here so that nothing can affect what we draw while we're drawing it
	std::vector<std::shared_ptr<SceneMesh>> meshes = std::vector<std::shared_ptr<SceneMesh>>( m_meshes );

	/*std::vector<glm::mat4> objectMatrices = {};
	for ( auto& sceneMesh : meshes )
	{
		objectMatrices.push_back( sceneMesh->m_transform.GetModelMatrix() );
	}*/

	for ( auto& sceneMesh : meshes )
	{
		bool materialWasDirty = false;

		for ( auto& m : sceneMesh->GetModel()->GetMeshes() )
		{
			// Check if material is dirty and create any resources
			if ( m.material->IsDirty() )
			{
				m.material->CreateResources();
				materialWasDirty = true;

				if ( !m.material->m_pipeline.IsValid() )
				{
					spdlog::error( "Material pipeline is INVALID even though we just created a pipeline!" );
					__debugbreak();
				}
			}

			if ( !m.material->m_pipeline.IsValid() )
			{
				spdlog::error( "Material pipeline was INVALID. Was material dirty? {}", materialWasDirty );
				__debugbreak();
			}

			Globals::m_renderContext->BindPipeline( m.material->m_pipeline );
			Globals::m_renderContext->BindDescriptor( m.material->m_descriptor );

			for ( int i = 0; i < m.material->m_textures.size(); ++i )
			{
				DescriptorUpdateInfo_t updateInfo = {};
				updateInfo.type = DESCRIPTOR_BINDING_TYPE_IMAGE;
				updateInfo.binding = i;
				updateInfo.src = &m.material->m_textures[i].m_image;

				Globals::m_renderContext->UpdateDescriptor( m.material->m_descriptor, updateInfo );
			}

			DescriptorUpdateInfo_t samplerUpdateInfo = {};
			samplerUpdateInfo.type = DESCRIPTOR_BINDING_TYPE_SAMPLER;
			samplerUpdateInfo.binding = m.material->m_textures.size();
			samplerUpdateInfo.samplerType = m.material->m_samplerType;

			Globals::m_renderContext->UpdateDescriptor( m.material->m_descriptor, samplerUpdateInfo );

			Globals::m_renderContext->BindVertexBuffer( m.vertexBuffer );

			RenderPushConstants meshConstants = RenderPushConstants( m_constants );

			meshConstants.modelMatrix = sceneMesh->m_transform.GetModelMatrix();
			meshConstants.renderMatrix = CalculateViewProjMatrix() * meshConstants.modelMatrix;

			Globals::m_renderContext->BindConstants( meshConstants );

			if ( m.isIndexed )
			{
				Globals::m_renderContext->BindIndexBuffer( m.indexBuffer );
				Globals::m_renderContext->Draw( m.vertices.count, m.indices.count, 1 );
			}
			else
			{
				Globals::m_renderContext->Draw( m.vertices.count, 0, 1 );
			}
		}
	}
}

void SceneMeshPass::AddMesh( std::shared_ptr<SceneMesh> sceneMesh )
{
	m_meshes.push_back( sceneMesh );
}

// todo: remove
void SceneMeshPass::SetConstants( RenderPushConstants constants )
{
	m_constants = constants;
}

void SceneMeshPass::RenderSceneMesh( SceneMesh* mesh )
{
}

void SceneMeshPass::RenderMesh( RenderPushConstants constants, Mesh* mesh )
{
}

void RenderManager::Render()
{
	// Server is headless - don't render
	if ( Globals::m_executingRealm == REALM_SERVER )
		return;

	//
	// 1. Queue passes
	//

	//
	// A. Scene mesh pass; renders all visible world objects in the scene
	//
	SceneMeshPass sceneMeshPass{};

	// Create and bind constants
	RenderPushConstants constants = {};
	constants.cameraPos = Globals::m_cameraPos.ToGLM();
	constants.time = Globals::m_curTime;
	constants.data.x = ( int )Globals::m_debugView;

	std::vector<Vector3> lightPositions = {};
	lightPositions.push_back( { 0, 4, 2 } );
	lightPositions.push_back( { 4, 4, 2 } );
	lightPositions.push_back( { 0, -4, 2 } );
	lightPositions.push_back( { -4, 4, 2 } );

	std::vector<glm::vec4> packedLightInfo = {};
	for ( int i = 0; i < 4; ++i )
	{
		packedLightInfo.push_back( { lightPositions[i].x, lightPositions[i].y, lightPositions[i].z, 10.0f } );
	}

	constants.vLightInfoWS[0] = packedLightInfo[0];
	constants.vLightInfoWS[1] = packedLightInfo[1];
	constants.vLightInfoWS[2] = packedLightInfo[2];
	constants.vLightInfoWS[3] = packedLightInfo[3];

	sceneMeshPass.SetConstants( constants );

	Globals::m_sceneGraph->ForEachSpecific<SceneMesh>( [&]( std::shared_ptr<SceneMesh> mesh ) {
		if ( ( mesh->GetFlags() & SCENE_MESH_FLAGS_WORLD_LAYER ) != 0 )
			sceneMeshPass.AddMesh( mesh );
	} );

	//
	// 2. Execute passes
	//
	Globals::m_renderContext->BeginRendering();
	sceneMeshPass.Execute();
	Globals::m_renderContext->EndRendering();

	Globals::m_hostManager->Render();
}

void RenderPass::Execute()
{
}

void RenderPass::SetInputTexture( RenderTexture texture )
{
}

void RenderPass::SetOutputTexture( RenderTexture texture )
{
}

TonemapPass::TonemapPass( std::shared_ptr<Material> material )
{
}

void TonemapPass::Execute()
{
}
