#pragma once
#include <globalvars.h>
#include <physicsmanager.h>

namespace Physics
{
	GENERATE_BINDINGS inline TraceResult Trace( TraceInfo traceInfo )
	{
		return g_physicsManager->Trace( traceInfo );
	}
}; // namespace Physics