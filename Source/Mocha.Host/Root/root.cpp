#include "root.h"

#include <Entities/entitymanager.h>
#include <Managed/hostmanager.h>
#include <Misc/defs.h>
#include <Misc/editormanager.h>
#include <Misc/globalvars.h>
#include <Misc/inputmanager.h>
#include <Misc/logmanager.h>
#include <Misc/projectmanager.h>
#include <Physics/physicsmanager.h>
#include <Rendering/renderdocmanager.h>
#include <Rendering/rendermanager.h>
#include <Root/serverroot.h>
#include <crtdbg.h>
#include <stdlib.h>

void Root::Startup()
{
	Globals::m_logManager = new LogManager();
	Globals::m_logManager->Startup();

	Globals::m_cvarManager = new CVarManager();
	Globals::m_cvarManager->Startup();

	Globals::m_projectManager = new ProjectManager();
	Globals::m_projectManager->Startup();

	Globals::m_sceneGraph = new SceneGraph();
	Globals::m_sceneGraph->Startup();

	Globals::m_physicsManager = new PhysicsManager();
	Globals::m_physicsManager->Startup();

	Globals::m_renderdocManager = new RenderdocManager();
	Globals::m_renderdocManager->Startup();

	Globals::m_inputManager = new InputManager();
	Globals::m_inputManager->Startup();

	Globals::m_renderManager = new RenderManager();
	Globals::m_renderManager->Startup();

	Globals::m_editorManager = new EditorManager();
	Globals::m_editorManager->Startup();

	Globals::m_hostManager = new HostManager();
	Globals::m_hostManager->Startup();
}

void Root::Shutdown()
{
	Globals::m_hostManager->Shutdown();
	Globals::m_editorManager->Shutdown();
	Globals::m_renderManager->Shutdown();
	Globals::m_inputManager->Shutdown();
	Globals::m_renderdocManager->Shutdown();
	Globals::m_physicsManager->Shutdown();
	Globals::m_sceneGraph->Shutdown();
	Globals::m_projectManager->Shutdown();
	Globals::m_cvarManager->Shutdown();
	Globals::m_logManager->Shutdown();
}

const char* Root::GetProjectPath()
{
	std::string str = EngineProperties::LoadedProject.GetValue();

	// Copy string so we can use it out-of-scope
	char* cstr = new char[str.length() + 1];
	strcpy_s( cstr, str.length() + 1, str.c_str() );

	return cstr;
}

double HiresTimeInSeconds()
{
	return std::chrono::duration_cast<std::chrono::duration<double>>(
	    std::chrono::high_resolution_clock::now().time_since_epoch() )
	    .count();
}

void Root::Run()
{
	Globals::m_hostManager->FireEvent( "Event.Game.Load" );

	double logicDelta = 1.0 / Globals::m_projectManager->GetProject().properties.tickRate;

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
			Globals::m_sceneGraph->ForEach(
			    [&]( std::shared_ptr<SceneMesh> mesh ) { mesh->m_transformLastFrame = mesh->m_transformCurrentFrame; } );

			Globals::m_tickDeltaTime = ( float )logicDelta;

			// Update physics
			Globals::m_physicsManager->Update();

			// Update game
			Globals::m_hostManager->Update();

			// TODO: Server / client
			// #ifndef DEDICATED_SERVER
			// Update window
			Globals::m_renderContext->UpdateWindow();
			// #endif

			if ( GetQuitRequested() )
			{
				m_shouldQuit = true;
				break;
			}

			// Assign current transforms to all entities
			Globals::m_sceneGraph->ForEach(
			    [&]( std::shared_ptr<SceneMesh> mesh ) { mesh->m_transformCurrentFrame = mesh->m_transform; } );

			Globals::m_curTime += logicDelta;
			accumulator -= logicDelta;
			Globals::m_curTick++;
		}

		Globals::m_frameDeltaTime = ( float )loopDeltaTime;

		// TODO: Server / client
		// #ifndef DEDICATED_SERVER
		// Render
		{
			const double alpha = accumulator / logicDelta;

			// Assign interpolated transforms to all entities
			Globals::m_sceneGraph->ForEach( [&]( std::shared_ptr<SceneMesh> mesh ) {
				// If this entity was spawned in just now, don't interpolate
				if ( mesh->m_spawnTime == Globals::m_curTick )
					return;

				mesh->m_transform =
				    Transform::Lerp( mesh->m_transformLastFrame, mesh->m_transformCurrentFrame, ( float )alpha );
			} );

			Globals::m_renderManager->DrawOverlaysAndEditor();

			Globals::m_renderManager->DrawGame();
		}
		// #endif
	}
}

Vector2 Root::GetWindowSize()
{
	Size2D size;
	Globals::m_renderContext->GetWindowSize( &size );
	return { ( float )size.x, ( float )size.y };
}

Vector2 Root::GetRenderSize()
{
	Size2D size;
	Globals::m_renderContext->GetRenderSize( &size );
	return { ( float )size.x, ( float )size.y };
}
