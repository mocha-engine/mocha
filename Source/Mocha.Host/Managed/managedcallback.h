#pragma once
#include <Managed/hostmanager.h>
#include <Misc/defs.h>
#include <Misc/globalvars.h>

class ManagedCallback
{
private:
	Handle m_handle{ HANDLE_INVALID };

public:
	ManagedCallback() {}

	GENERATE_BINDINGS ManagedCallback( Handle handle ) { m_handle = handle; }

	GENERATE_BINDINGS void Invoke()
	{
		if ( m_handle != HANDLE_INVALID )
			Globals::m_hostManager->InvokeCallback( m_handle );
	}
};
