#include "CMochaEngine.h"

#include "CEditor.h"
#include "CImgui.h"
#include "CWindow.h"
#include "Globals.h"

CMochaEngine::CMochaEngine()
{
	mWindow = std::make_unique<CWindow>( "Mocha", 1280, 720 );
	mRenderer = std::make_unique<CRenderer>( mWindow.get() );

	mWindow->AddObserver( mRenderer.get() );

	mImgui = std::make_unique<CImgui>( mWindow.get(), mRenderer.get() );
	g_Imgui = mImgui.get();
}

CMochaEngine::~CMochaEngine() {}

void CMochaEngine::Render()
{
	mRenderer->Render();

	// mImgui->NewFrame();
	// mEditor->Render();
	// mImgui->Render();
}

void CMochaEngine::Run()
{
	mEditor = std::make_unique<CEditor>();
	mWindow->Run( [&]() { Render(); } );
}