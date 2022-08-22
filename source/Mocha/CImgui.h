#pragma once
#include "Uint2.h"
#include "imgui.h"

#include <string>

class CWindow;
class CRenderer;
struct ID3D12GraphicsCommandList;

class CImgui
{
private:
	CWindow* mWindow;
	CRenderer* mRenderer;

public:
	ImFont* mMonospaceFont;
	ImFont* mSansSerifFont;
	ImFont* mBoldFont;
	ImFont* mHeadingFont;
	ImFont* mSubheadingFont;

	CImgui( CWindow* window, CRenderer* renderer );
	~CImgui();

	void NewFrame();
	void Render();
	void Resize( Uint2 newSize );
};