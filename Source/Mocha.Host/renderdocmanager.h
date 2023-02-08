#pragma once

#include <subsystem.h>

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
