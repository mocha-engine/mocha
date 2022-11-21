#pragma once

#include <subsystem.h>

class Renderdoc : ISubSystem
{
public:
	void StartUp();
	void ShutDown();
};
