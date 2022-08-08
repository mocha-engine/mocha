#pragma once
#include "generated/UnmanagedArgs.generated.h";

typedef void ( *imgui_render_fn )( void );
typedef void( CORECLR_DELEGATE_CALLTYPE* main_fn )( UnmanagedArgs* );

class CNativeWindow;
class CImgui;
class CRenderer;

class CEngine
{
private:
	std::unique_ptr<CNativeWindow> mWindow;
	std::unique_ptr<CImgui> mImgui;
	std::unique_ptr<CRenderer> mRenderer;

	imgui_render_fn mManagedRenderFunction;

	void Render();

public:
	CEngine();
	~CEngine();

	void Run();

	CNativeWindow* GetWindow();
	CImgui* GetImgui();
	CRenderer* GetRenderer();
};
