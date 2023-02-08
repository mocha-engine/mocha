#include "clientroot.h"

#include <baserendercontext.h>
#include <globalvars.h>

bool ClientRoot::GetQuitRequested()
{
	return m_renderContext->GetWindowCloseRequested();
}