#pragma once
#include <coreclr_delegates.h>
#include <memory>

class CWindow;
class CImgui;
class CRenderer;
class CEditor;

class CMochaEngine
{
private:
	std::unique_ptr<CWindow> mWindow;
	std::unique_ptr<CImgui> mImgui;
	std::unique_ptr<CRenderer> mRenderer;
	std::unique_ptr<CEditor> mEditor;

	void Render();

public:
	CMochaEngine();
	~CMochaEngine();

	void Run();

	inline CWindow* GetWindow() const { return mWindow.get(); }
	inline CImgui* GetImgui() const { return mImgui.get(); }
	inline CRenderer* GetRenderer() const { return mRenderer.get(); };
};
