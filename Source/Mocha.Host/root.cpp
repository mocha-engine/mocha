#include "root.h"

#include <crtdbg.h>
#include <defs.h>
#include <entitymanager.h>
#include <globalvars.h>
#include <hostmanager.h>
#include <inputmanager.h>
#include <logmanager.h>
#include <physicsmanager.h>
#include <projectmanager.h>
#include <renderdocmanager.h>
#include <rendermanager.h>
#include <stdlib.h>

// These global variables are all defined in globalvars.h,
// because the naming makes more sense (imagine if we
// included Root.h everywhere!)
RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;
EntityManager* g_entityDictionary;
PhysicsManager* g_physicsManager;
InputManager* g_inputManager;
BaseRenderContext* g_renderContext; // TODO: Remove
CVarManager* g_cvarManager;
ProjectManager* g_projectManager;

float g_curTime;
float g_frameDeltaTime;
float g_tickDeltaTime;
int g_curTick;
Vector3 g_cameraPos;
Quaternion g_cameraRot;
float g_cameraFov;
float g_cameraZNear;
float g_cameraZFar;
RenderDebugViews g_debugView;
Realm g_executingRealm;

namespace EngineProperties
{
	StringCVar LoadedProject(
	    "project.current", "Samples\\mocha-minimal\\project.json", CVarFlags::Archive, "Which project should we load?" );
	BoolCVar Raytracing( "render.raytracing", true, CVarFlags::Archive, "Enable raytracing" );
	BoolCVar Renderdoc( "render.renderdoc", false, CVarFlags::Archive, "Enable renderdoc" );
} // namespace EngineProperties

FloatCVar timescale( "game.timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );

// TODO: Server / client
extern FloatCVar maxFramerate;

void Root::Startup()
{
	g_logManager = new LogManager();
	g_logManager->Startup();

	g_cvarManager = new CVarManager();
	g_cvarManager->Startup();

	g_projectManager = new ProjectManager();
	g_projectManager->Startup();

	g_entityDictionary = new EntityManager();
	g_entityDictionary->Startup();

	g_physicsManager = new PhysicsManager();
	g_physicsManager->Startup();

	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->Startup();

	g_inputManager = new InputManager();
	g_inputManager->Startup();

	g_renderManager = new RenderManager();
	g_renderManager->Startup();

	g_hostManager = new HostManager();
	g_hostManager->Startup();
}

void Root::Shutdown()
{
	g_hostManager->Shutdown();

	g_renderManager->Shutdown();
	g_inputManager->Shutdown();
	g_renderdocManager->Shutdown();
	g_physicsManager->Shutdown();
	g_entityDictionary->Shutdown();
	g_projectManager->Shutdown();
	g_cvarManager->Shutdown();
	g_logManager->Shutdown();
}

double HiresTimeInSeconds()
{
	return std::chrono::duration_cast<std::chrono::duration<double>>(
	    std::chrono::high_resolution_clock::now().time_since_epoch() )
	    .count();
}

void Root::Run()
{
	g_hostManager->FireEvent( "Event.Game.Load" );

	double logicDelta = 1.0 / g_projectManager->GetProject().properties.tickRate;

	double currentTime = HiresTimeInSeconds();
	double accumulator = 0.0;

	while ( !m_shouldQuit )
	{
		double newTime = HiresTimeInSeconds();
		double loopDeltaTime = newTime - currentTime;

		// How quick did we do last frame? Let's limit ourselves if (1.0f / loopDeltaTime) is more than maxLoopHz
		float loopHz = 1.0f / loopDeltaTime;

		// TODO: Server / client. Perhaps abstract this and set it to the tickrate if we're a dedicated server?
		float maxLoopHz = maxFramerate.GetValue();

		if ( maxLoopHz > 0 && loopHz > maxLoopHz )
		{
			continue;
		}

		if ( loopDeltaTime > 1 / 30.0f )
			loopDeltaTime = 1 / 30.0f;

		currentTime = newTime;
		accumulator += loopDeltaTime;

		//
		// How long has it been since we last updated the game logic?
		// We want to update as many times as we can in this frame in
		// order to match the desired tick rate.
		//
		while ( accumulator >= logicDelta )
		{
			// Assign previous transforms to all entities
			g_entityDictionary->ForEach(
			    [&]( std::shared_ptr<BaseEntity> entity ) { entity->m_transformLastFrame = entity->m_transformCurrentFrame; } );

			g_tickDeltaTime = ( float )logicDelta;

			// Update physics
			g_physicsManager->Update();

			// Update game
			g_hostManager->Update();

			// TODO: Server / client
			// #ifndef DEDICATED_SERVER
			// Update window
			g_renderContext->UpdateWindow();
			// #endif

			if ( GetQuitRequested() )
			{
				m_shouldQuit = true;
				break;
			}

			// Assign current transforms to all entities
			g_entityDictionary->ForEach(
			    [&]( std::shared_ptr<BaseEntity> entity ) { entity->m_transformCurrentFrame = entity->m_transform; } );

			g_curTime += logicDelta;
			accumulator -= logicDelta;
			g_curTick++;
		}

		g_frameDeltaTime = ( float )loopDeltaTime;

		// TODO: Server / client
		// #ifndef DEDICATED_SERVER
		// Render
		{
			const double alpha = accumulator / logicDelta;

			// Assign interpolated transforms to all entities
			g_entityDictionary->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
				// If this entity was spawned in just now, don't interpolate
				if ( entity->m_spawnTime == g_curTick )
					return;

				entity->m_transform =
				    Transform::Lerp( entity->m_transformLastFrame, entity->m_transformCurrentFrame, ( float )alpha );
			} );

			g_renderManager->DrawOverlaysAndEditor();

			g_renderManager->DrawGame();
		}
		// #endif
	}
}

bool Root::GetQuitRequested()
{
	// TODO: Server / client
	// #ifdef DEDICATED_SERVER
	// ...
	// #else
	return g_renderContext->GetWindowCloseRequested();
	// #endif
}
