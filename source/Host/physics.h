#pragma once
#include <globalvars.h>
#include <physicsmanager.h>

//@InteropGen generate class
namespace Physics
{
	inline TraceResult Trace( TraceInfo traceInfo )
	{
		return g_physicsManager->Trace( traceInfo );
	}
}; // namespace Physics