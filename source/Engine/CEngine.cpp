#include "CEngine.h"

#include "CImgui.h"
#include "Globals.h"

CEngine::CEngine()
{
	mWindow = std::make_unique<CNativeWindow>( "Mocha", 1280, 720 );
	mRenderer = std::make_unique<CRenderer>( mWindow.get() );

	mImgui = std::make_unique<CImgui>( mWindow.get(), mRenderer.get() );
	g_Imgui = mImgui.get();
}

void CEngine::Render()
{
	mRenderer->BeginFrame();

	auto cl = mRenderer->GetCommandList();
	mImgui->NewFrame();

	mManagedRenderFunction();

	mImgui->Render( cl );
	mRenderer->EndFrame();
}

void CEngine::Run()
{
	char_t host_path[MAX_PATH];
	auto size = GetCurrentDirectory( MAX_PATH, host_path );
	assert( size != 0 );

	string_t root_path = host_path;
	string_t engine_dir = root_path + STR( "\\" );

	CNetCoreHost net_core_host( engine_dir + STR( "Editor.runtimeconfig.json" ), engine_dir + STR( "Editor.dll" ) );

	// clang-format off
	main_fn mainFunction = (main_fn)net_core_host.FindFunction(
		STR("Mocha.Engine.Program, Editor"),
		STR("HostedMain")
	);
	
	mManagedRenderFunction = (imgui_render_fn)net_core_host.FindFunction(
		STR("Mocha.Engine.Program, Editor"),
		STR("Render")
	);
	// clang-format on

	mainFunction( &args );
	mWindow->Run( [&]() { Render(); } );
}

CNativeWindow* CEngine::GetWindow()
{
	return mWindow.get();
}

CImgui* CEngine::GetImgui()
{
	return mImgui.get();
}

CRenderer* CEngine::GetRenderer()
{
	return mRenderer.get();
}

CEngine::~CEngine() {}
