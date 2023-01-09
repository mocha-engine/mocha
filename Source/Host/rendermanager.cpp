#include "rendermanager.h"

//
//
//
#include <baseentity.h>
#include <cvarmanager.h>
#include <defs.h>
#include <entitymanager.h>
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

FloatCVar timescale( "game.timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
FloatCVar maxFramerate(
    "render.max_framerate", 144.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

void RenderManager::RenderMesh( RenderPushConstants constants, Mesh* mesh )
{
	// JIT pipeline creation
	if ( !mesh->material.m_pipeline.IsValid() )
	{
		spdlog::trace( "RenderManager::RenderMesh - Handle wasn't valid, creating JIT render pipeline..." );

		mesh->material.CreateResources();
	}

	m_renderContext->BindPipeline( mesh->material.m_pipeline );
	m_renderContext->BindDescriptor( mesh->material.m_descriptor );

	for ( int i = 0; i < mesh->material.m_textures.size(); ++i )
	{
		DescriptorUpdateInfo_t updateInfo = {};
		updateInfo.binding = i;
		updateInfo.samplerType = SAMPLER_TYPE_POINT;
		updateInfo.src = &mesh->material.m_textures[i].m_image;

		m_renderContext->UpdateDescriptor( mesh->material.m_descriptor, updateInfo );
	}

	m_renderContext->BindConstants( constants );
	m_renderContext->BindVertexBuffer( mesh->vertexBuffer );
	m_renderContext->BindIndexBuffer( mesh->indexBuffer );

	m_renderContext->Draw( mesh->vertices.count, mesh->indices.count, 1 );
}

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
		RenderMesh( constants, &mesh );
	}
}

void RenderManager::Render()
{
	RenderStatus res = m_renderContext->BeginRendering();

	if ( res == RENDER_STATUS_WINDOW_SIZE_INVALID )
		return;

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

	m_renderContext->EndRendering();
}

double HiresTimeInSeconds()
{
	return std::chrono::duration_cast<std::chrono::duration<double>>(
	    std::chrono::high_resolution_clock::now().time_since_epoch() )
	    .count();
}

void RenderManager::Run()
{
	bool bQuit = false;

	g_hostManager->FireEvent( "Event.Game.Load" );

	const int ticksPerSecond = 40; // TODO: convar? ideally shouldn't be changed mid-game

	double curTime = 0.0;
	double logicDelta = 1.0 / ticksPerSecond;

	double currentTime = HiresTimeInSeconds();
	double accumulator = 0.0;

	while ( !bQuit )
	{
		double newTime = HiresTimeInSeconds();
		double frameTime = newTime - currentTime;

		// How quick did we do last frame? Let's limit ourselves if (1.0f / g_frameTime) is more than maxFramerate
		float fps = 1.0f / frameTime;
		float maxFps = maxFramerate.GetValue();

		if ( maxFps > 0 && fps > maxFps )
		{
			continue;
		}

		if ( frameTime > 0.25 )
			frameTime = 0.25;

		currentTime = newTime;
		accumulator += frameTime;

		//
		// How long has it been since we last updated the game logic?
		// We want to update as many times as we can in this frame in
		// order to match the desired tick rate.
		//
		while ( accumulator >= logicDelta )
		{
			g_tickTime = ( float )logicDelta;

			// Update physics
			g_physicsManager->Update();

			// Update game
			g_hostManager->Update();

			// Update window
			if ( m_renderContext->UpdateWindow() == RENDER_STATUS_WINDOW_CLOSE )
			{
				bQuit = true;
				break;
			}

			curTime += logicDelta;
			accumulator -= logicDelta;
			g_curTick++;
		}

		g_frameTime = ( float )frameTime;

		// Render
		{
			// Draw editor
			{
				m_renderContext->BeginImGui();
				ImGui::NewFrame();
				ImGui::DockSpaceOverViewport( nullptr, ImGuiDockNodeFlags_PassthruCentralNode );

				g_hostManager->Render();
				g_hostManager->DrawEditor();

				m_renderContext->EndImGui();
			}

			// Draw game
			Render();
		}
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
