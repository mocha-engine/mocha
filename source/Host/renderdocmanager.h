#pragma once

#include <subsystem.h>

class RenderdocManager : ISubSystem
{
public:
	void StartUp();
	void ShutDown();
};
