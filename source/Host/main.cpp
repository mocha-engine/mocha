#include "managed/managedhost.h"
#include "vulkan/engine.h"

#include <SDL2/SDL.h>
#include <iostream>
#include <root.h>
#undef main

#include <globalvars.h>

int main()
{
	Root::GetInstance().StartUp();

	g_engine->Run( g_managedHost );

	Root::GetInstance().ShutDown();

	return 0;
}