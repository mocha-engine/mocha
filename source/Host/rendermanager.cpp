#include "rendermanager.h"

//
//
//
#include <baseentity.h>
#include <cvarmanager.h>
#include <defs.h>
#include <edict.h>
#include <fontawesome.h>
#include <gamesettings.h>
#include <globalvars.h>
#include <hostmanager.h>
#include <modelentity.h>
#include <physicsmanager.h>
#include <shadercompiler.h>
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

FloatCVar timescale( "timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
FloatCVar maxFramerate( "fps_max", 144.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

void RenderManager::Startup()
{
	g_renderManager = this;

	m_renderContext = std::make_unique<VulkanRenderContext>();
	g_renderContext = m_renderContext.get();

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
	constants.modelMatrix = entity->GetTransform().GetModelMatrix();
	constants.renderMatrix = CalculateViewProjMatrix() * constants.modelMatrix;
	constants.cameraPos = g_cameraPos.ToGLM();
	constants.time = g_curTime;
	constants.data.x = ( int )g_debugView;

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
		m_renderContext->RenderMesh( constants, &mesh );
	}
}

void RenderManager::Render()
{
	m_renderContext->BeginRendering();

	auto viewProjMatrix = CalculateViewProjMatrix();
	auto viewmodelViewProjMatrix = CalculateViewmodelViewProjMatrix();

	g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( !entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) && !entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render viewmodels
	//
	g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) )
			RenderEntity( entity.get() );
	} );

	//
	// Render UI last
	//
	g_entityDictionary->ForEachSpecific<ModelEntity>( [&]( std::shared_ptr<ModelEntity> entity ) {
		if ( entity->HasFlag( EntityFlags::ENTITY_UI ) )
			RenderEntity( entity.get() );
	} );

	//
	// Editor UI
	//
	m_renderContext->RenderImGui();

	m_renderContext->EndRendering();
}

void RenderManager::Run()
{
	bool bQuit = false;

	g_hostManager->FireEvent( "Event.Game.Load" );

	while ( !bQuit )
	{
		static auto gameStart = std::chrono::steady_clock::now();
		static float flFilteredTime = 0;
		static float flPreviousTime = 0;
		static float flFrameTime = 0;

		std::chrono::duration<float> timeSinceStart = std::chrono::steady_clock::now() - gameStart;
		float flCurrentTime = timeSinceStart.count();

		float dt = flCurrentTime - flPreviousTime;
		flPreviousTime = flCurrentTime;

		flFrameTime += dt;

		if ( flFrameTime < 0.0f )
			return;

		// How quick did we do last frame? Let's limit ourselves if (1.0f / g_frameTime) is more than maxFramerate
		float fps = 1.0f / flFrameTime;
		float maxFps = maxFramerate.GetValue();

		if ( maxFps > 0 && fps > maxFps )
		{
			flFilteredTime += g_frameTime;
			continue;
		}

		g_curTime = flCurrentTime;
		g_frameTime = flFrameTime;

		flFilteredTime = 0;
		flFrameTime = 0;

		m_renderContext->UpdateWindow();
		
		m_renderContext->BeginImGui();

		{
			ImGui::NewFrame();
			ImGui::DockSpaceOverViewport( nullptr, ImGuiDockNodeFlags_PassthruCentralNode );

			g_hostManager->DrawEditor();
		}

		g_physicsManager->Update();
		g_hostManager->Render();
		
		m_renderContext->EndImGui();

		Render();
	}
}

glm::mat4 RenderManager::CalculateViewmodelViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( 60.0f ), aspect, g_cameraZNear, g_cameraZFar );

	return projMatrix * viewMatrix;
}

glm::mat4 RenderManager::CalculateViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.x / ( float )extent.y;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( g_cameraFov ), aspect, g_cameraZNear, g_cameraZFar );

	return projMatrix * viewMatrix;
}
