#pragma once
#include <defs.h>
#include <globalvars.h>
#include <projectmanager.h>
#include <projectmanifest.h>
#include <clientroot.h>

namespace Engine
{
	GENERATE_BINDINGS inline void Quit()
	{
		auto& root = ClientRoot::GetInstance();
		root.Quit();
	}

	GENERATE_BINDINGS inline int GetCurrentTick()
	{
		auto& root = ClientRoot::GetInstance();
		return root.g_curTick;
	}

	GENERATE_BINDINGS inline float GetFrameDeltaTime()
	{
		auto& root = ClientRoot::GetInstance();
		return root.g_frameDeltaTime;
	}

	GENERATE_BINDINGS inline float GetTickDeltaTime()
	{
		auto& root = ClientRoot::GetInstance();
		return root.g_tickDeltaTime;
	}

	GENERATE_BINDINGS inline float GetFramesPerSecond()
	{
		auto& root = ClientRoot::GetInstance();
		return 1.0f / root.g_frameDeltaTime;
	}

	GENERATE_BINDINGS inline float GetTime()
	{
	    auto& root = ClientRoot::GetInstance();
		return root.g_curTime;
	}

	GENERATE_BINDINGS inline const char* GetProjectPath()
	{
		std::string str = EngineProperties::LoadedProject.GetValue();

		// Copy string so we can use it out-of-scope
		char* cstr = new char[str.length() + 1];
		strcpy_s( cstr, str.length() + 1, str.c_str() );

		return cstr;
	};

	GENERATE_BINDINGS inline bool IsServer()
    {
		return IS_SERVER;
	}

	GENERATE_BINDINGS inline bool IsClient()
    {
		return IS_CLIENT;
	}
}; // namespace Engine
