#pragma once
#include <coreclr_delegates.h>
#include <memory>

class CWindow;
class CImgui;
class CRenderer;
class CEditor;

class CEngine
{
private:
	std::unique_ptr<CWindow> mWindow;
	std::unique_ptr<CImgui> mImgui;
	std::unique_ptr<CRenderer> mRenderer;
	std::unique_ptr<CEditor> mEditor;

	void Render();

public:
	CEngine();
	~CEngine();

	void Run();

	CWindow* GetWindow();
	CImgui* GetImgui();
	CRenderer* GetRenderer();
};
