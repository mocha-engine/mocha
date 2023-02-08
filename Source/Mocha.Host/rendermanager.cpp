#include "rendermanager.h"

//
//
//
#include <baseentity.h>
#include <cvarmanager.h>
#include <defs.h>
#include <entitymanager.h>
#include <fontawesome.h>
#include <clientroot.h>
#include <hostmanager.h>
#include <modelentity.h>
#include <physicsmanager.h>
#include <projectmanager.h>
#include <vulkanrendercontext.h>

//
//
//
#include <algorithm>
#include <fstream>
#include <iostream>
#include <memory>
#include <window.h>

//
//
//
#include <glm/ext.hpp>
#include <spdlog/spdlog.h>

//
//
//
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <imgui.h>
#include <implot.h>
#include <nullrendercontext.h>

FloatCVar maxFramerate(
    "render.max_framerate", 144.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

void RenderManager::RenderMesh( RenderPushConstants constants, Mesh* mesh )
{
	bool materialWasDirty = false;

	// Check if material is dirty and create any resources
	if ( mesh->material->IsDirty() )
	{
		mesh->material->CreateResources();
		materialWasDirty = true;

		if ( !mesh->material->m_pipeline.IsValid() )
		{
			spdlog::error( "Material pipeline is INVALID even though we just created a pipeline!" );
			__debugbreak();
		}
	}

	if ( !mesh->material->m_pipeline.IsValid() )
	{
		spdlog::error( "Material pipeline was INVALID. Was material dirty? {}", materialWasDirty );
		__debugbreak();
	}

	m_renderContext->BindPipeline( mesh->material->m_pipeline );
	m_renderContext->BindDescriptor( mesh->material->m_descriptor );

	for ( int i = 0; i < mesh->material->m_textures.size(); ++i )
	{
		DescriptorUpdateInfo_t updateInfo = {};
		updateInfo.binding = i;
		updateInfo.samplerType = SAMPLER_TYPE_POINT;
		updateInfo.src = &mesh->material->m_textures[i].m_image;

		m_renderContext->UpdateDescriptor( mesh->material->m_descriptor, updateInfo );
	}

	m_renderContext->BindConstants( constants );
	m_renderContext->BindVertexBuffer( mesh->vertexBuffer );
	m_renderContext->BindIndexBuffer( mesh->indexBuffer );

	m_renderContext->Draw( mesh->vertices.count, mesh->indices.count, 1 );
}

void RenderManager::Startup()
{
	auto& root = ClientRoot::GetInstance();
	root.g_renderManager = this;

	if ( IS_CLIENT )
	{
		// Client uses Vulkan for rendering
		m_renderContext = std::make_unique<VulkanRenderContext>();
	}
	else
	{
		// Server is headless - use a null render context
		m_renderContext = std::make_unique<NullRenderContext>();
	}

	root.g_renderContext = m_renderContext.get();

	m_renderContext->Startup();
}

void RenderManager::Shutdown()
{
	m_renderContext->Shutdown();
}

void RenderManager::RenderEntity( ModelEntity* entity )
{
	auto& root = ClientRoot::GetInstance();
	
	// Create and bind constants
	RenderPushConstants constants = {};
	constants.modelMatrix = entity->m_transform.GetModelMatrix();
	constants.renderMatrix = CalculateViewProjMatrix() * constants.modelMatrix;
	constants.cameraPos = root.g_cameraPos.ToGLM();
	constants.time = root.g_curTime;
	constants.data.x = ( int )root.g_debugView;

	std::vector<Vector3> lightPositions = {};
	lightPositions.push_back( { 0, 4, 4 } );
	lightPositions.push_back( { 4, 0, 4 } );
	lightPositions.push_back( { 0, -4, 4 } );
	lightPositions.push_back( { -4, 0, 4 } );

	std::vector<glm::vec4> packedLightInfo = {};
	for ( int i = 0; i < 4; ++i )
	{
		packedLightInfo.push_back( { lightPositions[i].x, lightPositions[i].y, lightPositions[i].z, 50.0f } );
	}

	constants.vLightInfoWS[0] = packedLightInfo[0];
	constants.vLightInfoWS[1] = packedLightInfo[1];
	constants.vLightInfoWS[2] = packedLightInfo[2];
	constants.vLightInfoWS[3] = packedLightInfo[3];

	for ( auto& mesh : entity->GetModel()->m_meshes )
	{
		RenderMesh( constants, &mesh );
	}
}

void RenderManager::DrawOverlaysAndEditor()
{
	// Server is headless - no overlays or editor
	if ( IS_SERVER )
		return;

	auto& root = ClientRoot::GetInstance();
	
	m_renderContext->BeginImGui();
	ImGui::NewFrame();
	ImGui::DockSpaceOverViewport( nullptr, ImGuiDockNodeFlags_PassthruCentralNode );

	root.g_hostManager->Render();
	root.g_hostManager->DrawEditor();

	m_renderContext->EndImGui();
}

void RenderManager::DrawGame()
{
	// Server is headless - don't render
	if ( IS_SERVER )
		return;

	auto& root = ClientRoot::GetInstance();
	RenderStatus res = m_renderContext->BeginRendering();

	if ( res == RENDER_STATUS_WINDOW_SIZE_INVALID )
		return;

	auto viewProjMatrix = CalculateViewProjMatrix();
	auto viewmodelViewProjMatrix = CalculateViewmodelViewProjMatrix();

	root.g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( !entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) && !entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render viewmodels
	//
	root.g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render UI last
	//
	root.g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	m_renderContext->EndRendering();
}

glm::mat4 RenderManager::CalculateViewmodelViewProjMatrix()
{
	auto& root = ClientRoot::GetInstance();
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( root.g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = root.g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( 60.0f ), aspect, root.g_cameraZNear, root.g_cameraZFar );

	return projMatrix * viewMatrix;
}

glm::mat4 RenderManager::CalculateViewProjMatrix()
{
	auto& root = ClientRoot::GetInstance();
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( root.g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = root.g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( root.g_cameraFov ), aspect, root.g_cameraZNear, root.g_cameraZFar );

	return projMatrix * viewMatrix;
}
