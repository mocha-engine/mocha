#include "clientroot.h"

#include <Misc/globalvars.h>
#include <Rendering/baserendercontext.h>

bool ClientRoot::GetQuitRequested()
{
	return m_renderContext->GetWindowCloseRequested();
}