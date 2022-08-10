#include "CEngine.h"

#include "CEditor.h"
#include "CImgui.h"
#include "CWindow.h"
#include "Globals.h"

CEngine::CEngine()
{
	mWindow = std::make_unique<CWindow>( "Mocha", 1280, 720 );
	mRenderer = std::make_unique<CRenderer>( mWindow.get() );

	mImgui = std::make_unique<CImgui>( mWindow.get(), mRenderer.get() );
	g_Imgui = mImgui.get();
}

void CEngine::Render()
{
	mRenderer->BeginFrame();

	auto cl = mRenderer->GetCommandList();
	mImgui->NewFrame();

	mEditor->Render();

	mImgui->Render( cl );
	mRenderer->EndFrame();
}

void CEngine::Run()
{
	mEditor = std::make_unique<CEditor>();
	mWindow->Run( [&]() { Render(); } );
}

CWindow* CEngine::GetWindow()
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
