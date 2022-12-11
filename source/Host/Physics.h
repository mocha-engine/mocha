#pragma once
#include <globalvars.h>
#include <physicsmanager.h>

//@InteropGen generate class
namespace Physics
{
	inline TraceResult TraceRay( Vector3 startPos, Vector3 endPos )
	{
		return g_physicsManager->TraceRay( startPos, endPos );
	}
}; // namespace Physics