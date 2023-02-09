#pragma once
#include <clientroot.h>
#include <physicsmanager.h>

namespace Physics
{
	GENERATE_BINDINGS inline TraceResult Trace( TraceInfo traceInfo )
	{
		return FindInstance().m_physicsManager->Trace( traceInfo );
	}
}; // namespace Physics