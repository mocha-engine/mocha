#pragma once
#include <defs.h>
#include <globalvars.h>
#include <root.h>
#include <projectmanager.h>
#include <projectmanifest.h>

namespace Engine
{
	GENERATE_BINDINGS inline void Quit()
	{
		auto& root = Root::GetInstance();
		root.Quit();
	}

	GENERATE_BINDINGS inline int GetCurrentTick()
	{
		return g_curTick;
	}

	GENERATE_BINDINGS inline float GetDeltaTime()
	{
		return g_frameTime;
	}

	GENERATE_BINDINGS inline float GetTickDeltaTime()
	{
		return g_tickTime;
	}

	GENERATE_BINDINGS inline float GetFramesPerSecond()
	{
		return 1.0f / g_frameTime;
	}

	GENERATE_BINDINGS inline float GetTime()
	{
		return g_curTime;
	}

	GENERATE_BINDINGS inline const char* GetProjectPath()
	{
		std::string str = EngineProperties::LoadedProject.GetValue();
		
		// Copy string so we can use it out-of-scope
		char* cstr = new char[str.length() + 1];
		strcpy_s( cstr, str.length() + 1, str.c_str() );
				
		return cstr;
	};
}; // namespace Engine
