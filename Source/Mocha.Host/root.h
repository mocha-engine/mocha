#pragma once
#include <subsystem.h>

class Root : ISubSystem
{
protected:
	bool m_shouldQuit = false;
	virtual bool GetQuitRequested() = 0;

public:
	void Startup();
	void Run();
	void Shutdown();

	void Quit() { m_shouldQuit = true; }
};
