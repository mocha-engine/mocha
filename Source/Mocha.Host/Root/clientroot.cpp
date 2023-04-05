#include "clientroot.h"

#include <Misc/globalvars.h>
#include <Rendering/baserendercontext.h>

bool ClientRoot::GetQuitRequested()
{
	return Globals::m_renderContext->GetWindowCloseRequested();
}