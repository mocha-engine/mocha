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
#include <mesh.h>
#include <modelentity.h>
#include <physicsmanager.h>
#include <shadercompiler.h>
#include <vk_types.h>
#include <vkinit.h>
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

#ifdef _IMGUI
#include <backends/imgui_impl_sdl.h>
#include <backends/imgui_impl_vulkan.h>
#include <imgui.h>
#include <implot.h>
#endif

FloatCVar timescale( "timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
FloatCVar maxFramerate( "fps_max", 144.0f, CVarFlags::Archive, "The maximum framerate at which the game should run." );

VkBool32 DebugCallback( VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageTypes,
    const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData )
{
	const std::shared_ptr<spdlog::logger> logger = spdlog::get( "renderer" );

	switch ( messageSeverity )
	{
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT:
		logger->trace( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT:
		logger->info( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT:
		logger->warn( pCallbackData->pMessage );
		break;
	case VkDebugUtilsMessageSeverityFlagBitsEXT::VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT:
		logger->error( pCallbackData->pMessage );
		break;
	}

	return VK_FALSE;
}

void RenderManager::Startup()
{
	g_renderManager = this;
	
	m_renderContext = std::make_unique<VulkanRenderContext>();
	m_renderContext->Startup();
}

void RenderManager::Shutdown()
{
	m_renderContext->Shutdown();
}

void RenderManager::Render()
{
	/*
	 * RenderContext::BeginRendering();
	 * // The above should auto-bind to the main render pass
	 *
	 * // We now want to bind scene data
	 * RenderContext::BindUniformBuffer( sceneData );
	 *
	 * For each world entity:
	 *	entity->render:
	 *	For each mesh:
	 *	- RenderContext::BindVertexBuffer( mesh->vertexBuffer );
	 *  - RenderContext::BindIndexBuffer( mesh->indexBuffer );
	 *  - RenderContext::BindPipeline( mesh->pipeline );
	 *	- RenderContext::Draw( mesh->vertCount, mesh->indexCount, 1 );
	 *
	 * // Update scene data and re-bind
	 * sceneData.renderMatrix = ...;
	 *
	 * // BindUniformBuffer should *automatically update the internal buffer*.
	 * // This means we only have to do one call to RenderContext to update the scene data.
	 * RenderContext::BindUniformBuffer( sceneData );
	 *
	 * For each viewmodel entity:
	 *	entity->render
	 *
	 * Swap to the UI render pass:
	 * RenderContext::BindRenderTarget( RenderContext::Pass::UI );
	 * ( Could do with being a better API )
	 *
	 * For each UI entity:
	 *	entity->render
	 *
	 * // Finish, submit, present
	 * RenderContext::EndRendering();
	 */

	m_renderContext->BeginRendering();

	auto viewProjMatrix = CalculateViewProjMatrix();
	auto viewmodelViewProjMatrix = CalculateViewmodelViewProjMatrix();

	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( !renderEntity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) && !renderEntity->HasFlag( EntityFlags::ENTITY_UI ) )
				m_renderContext->RenderEntity( renderEntity.get() );
		}
	} );

	//
	// Render viewmodels
	//
	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( renderEntity->HasFlag( EntityFlags::ENTITY_VIEWMODEL ) )
				m_renderContext->RenderEntity( renderEntity.get() );
		}
	} );

	//
	// Render UI last
	//
	g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
		auto renderEntity = std::dynamic_pointer_cast<ModelEntity>( entity );
		if ( renderEntity != nullptr )
		{
			if ( renderEntity->HasFlag( EntityFlags::ENTITY_UI ) )
				m_renderContext->RenderEntity( renderEntity.get() );
		}
	} );

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

		g_physicsManager->Update();
		g_hostManager->Render();

		Render();
	}
}

glm::mat4 RenderManager::CalculateViewmodelViewProjMatrix()
{
	glm::mat4 viewMatrix, projMatrix;

	auto extent = GetWindowExtent();
	float aspect = ( float )extent.width / ( float )extent.height;

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
	float aspect = ( float )extent.width / ( float )extent.height;

	glm::vec3 up = glm::vec3( 0, 0, -1 );
	glm::vec3 direction = glm::normalize( glm::rotate( g_cameraRot.ToGLM(), glm::vec3( 1, 0, 0 ) ) );
	glm::vec3 position = g_cameraPos.ToGLM();

	viewMatrix = glm::lookAt( position, position + direction, up );
	projMatrix = glm::perspective( glm::radians( g_cameraFov ), aspect, g_cameraZNear, g_cameraZFar );

	return projMatrix * viewMatrix;
}
