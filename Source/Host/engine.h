#pragma once
#include <defs.h>
#include <globalvars.h>

namespace Engine
{
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
}; // namespace Engine
