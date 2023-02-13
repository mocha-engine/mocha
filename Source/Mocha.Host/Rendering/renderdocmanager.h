#pragma once

#include <Misc/subsystem.h>

class RenderdocManager : ISubSystem
{
public:
	RenderdocManager( Root* parent )
	    : ISubSystem( parent )
	{
	}

	void Startup();
	void Shutdown();
};
