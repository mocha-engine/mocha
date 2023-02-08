#include "clientroot.h"

#include <baserendercontext.h>
#include <globalvars.h>

bool ClientRoot::GetQuitRequested()
{
	return g_renderContext->GetWindowCloseRequested();
}