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
	FindInstance().m_renderManager = this;

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

	FindInstance().m_renderContext = m_renderContext.get();

	m_renderContext->Startup();
}

void RenderManager::Shutdown()
{
	m_renderContext->Shutdown();
}

void RenderManager::RenderEntity( ModelEntity* entity )
{	
	// Create and bind constants
	RenderPushConstants constants = {};
	constants.modelMatrix = entity->m_transform.GetModelMatrix();
	constants.renderMatrix = CalculateViewProjMatrix() * constants.modelMatrix;
	constants.cameraPos = FindInstance().m_cameraPos.ToGLM();
	constants.time = FindInstance().m_curTime;
	constants.data.x = ( int )FindInstance().m_debugView;

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

	m_renderContext->BeginImGui();
	ImGui::NewFrame();
	ImGui::DockSpaceOverViewport( nullptr, ImGuiDockNodeFlags_PassthruCentralNode );

	FindInstance().m_hostManager->Render();
	FindInstance().m_hostManager->DrawEditor();

	m_renderContext->EndImGui();
}

void RenderManager::DrawGame()
{
	// Server is headless - don't render
	if ( IS_SERVER )
		return;
	
	RenderStatus res = m_renderContext->BeginRendering();

	if ( res == RENDER_STATUS_WINDOW_SIZE_INVALID )
		return;

	auto viewProjMatrix = CalculateViewProjMatrix();
	auto viewmodelViewProjMatrix = CalculateViewmodelViewProjMatrix();

	FindInstance().m_entityManager->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( !entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) && !entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render viewmodels
	//
	FindInstance().m_entityManager->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render UI last
	//
	FindInstance().m_entityManager->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	m_renderContext->EndRendering();
}

glm::mat4 RenderManager::CalculateViewmodelViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( FindInstance().m_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = FindInstance().m_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( 60.0f ), aspect, FindInstance().m_cameraZNear, FindInstance().m_cameraZFar );

	return projMatrix * viewMatrix;
}

glm::mat4 RenderManager::CalculateViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( FindInstance().m_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = FindInstance().m_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective(
	    glm::radians( FindInstance().m_cameraFov ), aspect, FindInstance().m_cameraZNear, FindInstance().m_cameraZFar );

	return projMatrix * viewMatrix;
}
