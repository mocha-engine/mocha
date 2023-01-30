#pragma once

#include <cvarmanager.h>
#include <defs.h>
#include <globalvars.h>
#include <sstream>
#include <string>

namespace ConsoleSystem
{
	GENERATE_BINDINGS inline void Run( const char* command )
	{
		CVarSystem::Instance().Run( command );
	}
} // namespace ConsoleSystem