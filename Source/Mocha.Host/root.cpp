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

void Root::Startup()
{
	m_logManager = new LogManager();
	m_logManager->Startup();

	m_cvarManager = new CVarManager();
	m_cvarManager->Startup();

	m_projectManager = new ProjectManager();
	m_projectManager->Startup();

	m_entityManager = new EntityManager();
	m_entityManager->Startup();

	m_physicsManager = new PhysicsManager();
	m_physicsManager->Startup();

	m_renderdocManager = new RenderdocManager();
	m_renderdocManager->Startup();

	m_inputManager = new InputManager();
	m_inputManager->Startup();

	m_renderManager = new RenderManager();
	m_renderManager->Startup();

	m_hostManager = new HostManager();
	m_hostManager->Startup();
}

void Root::Shutdown()
{
	m_hostManager->Shutdown();

	m_renderManager->Shutdown();
	m_inputManager->Shutdown();
	m_renderdocManager->Shutdown();
	m_physicsManager->Shutdown();
	m_entityManager->Shutdown();
	m_projectManager->Shutdown();
	m_cvarManager->Shutdown();
	m_logManager->Shutdown();
}

double HiresTimeInSeconds()
{
	return std::chrono::duration_cast<std::chrono::duration<double>>(
	    std::chrono::high_resolution_clock::now().time_since_epoch() )
	    .count();
}

void Root::Run()
{
	m_hostManager->FireEvent( "Event.Game.Load" );

	double logicDelta = 1.0 / m_projectManager->GetProject().properties.tickRate;

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
			m_entityManager->ForEach(
			    [&]( std::shared_ptr<BaseEntity> entity ) { entity->m_transformLastFrame = entity->m_transformCurrentFrame; } );

			m_tickDeltaTime = ( float )logicDelta;

			// Update physics
			m_physicsManager->Update();

			// Update game
			m_hostManager->Update();

			// TODO: Server / client
			// #ifndef DEDICATED_SERVER
			// Update window
			m_renderContext->UpdateWindow();
			// #endif

			if ( GetQuitRequested() )
			{
				m_shouldQuit = true;
				break;
			}

			// Assign current transforms to all entities
			m_entityManager->ForEach(
			    [&]( std::shared_ptr<BaseEntity> entity ) { entity->m_transformCurrentFrame = entity->m_transform; } );

			m_curTime += logicDelta;
			accumulator -= logicDelta;
			m_curTick++;
		}

		m_frameDeltaTime = ( float )loopDeltaTime;

		// TODO: Server / client
		// #ifndef DEDICATED_SERVER
		// Render
		{
			const double alpha = accumulator / logicDelta;

			// Assign interpolated transforms to all entities
			m_entityManager->ForEach( [&]( std::shared_ptr<BaseEntity> entity ) {
				// If this entity was spawned in just now, don't interpolate
				if ( entity->m_spawnTime == m_curTick )
					return;

				entity->m_transform =
				    Transform::Lerp( entity->m_transformLastFrame, entity->m_transformCurrentFrame, ( float )alpha );
			} );

			m_renderManager->DrawOverlaysAndEditor();

			m_renderManager->DrawGame();
		}
		// #endif
	}
}
