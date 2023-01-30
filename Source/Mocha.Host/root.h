#pragma once
#include <subsystem.h>

class Root : ISubSystem
{
private:
	bool m_shouldQuit = false;

	bool GetQuitRequested();

public:
	inline static Root& GetInstance()
	{
		static Root instance;
		return instance;
	}

	void Startup();
	void Run();
	void Shutdown();

	void Quit() { m_shouldQuit = true; }
};
