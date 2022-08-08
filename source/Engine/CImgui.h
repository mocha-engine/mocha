#pragma once
#include "imgui.h"
#include "imgui_impl_dx12.h"
#include "imgui_impl_sdl.h"

#include <string>

class CNativeWindow;
class CRenderer;
struct ID3D12GraphicsCommandList;

class CImgui
{
private:
	CNativeWindow* mWindow;
	CRenderer* mRenderer;

public:
	ImFont* mMonospaceFont;
	ImFont* mSansSerifFont;
	ImFont* mBoldFont;
	ImFont* mHeadingFont;
	ImFont* mSubheadingFont;

	CImgui( CNativeWindow* window, CRenderer* renderer );
	~CImgui();
	void NewFrame();
	void Render( ID3D12GraphicsCommandList* commandList );
};