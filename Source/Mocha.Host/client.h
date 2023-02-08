#pragma once
#include <defs.h>
#include <globalvars.h>
#include <serverroot.h>

namespace Client
{
	GENERATE_BINDINGS inline void StartListenServer()
	{
		if ( g_executingRealm == REALM_SERVER )
			return; // Not allowed on dedicated server or on existing listen server

		auto serverRoot = ServerRoot::GetInstance();

		spdlog::info( "Spinning up new root for listen server... ( we should do this some other way )" );
	}
}; // namespace Client