#include "managedcallback.h"
#include <Managed/hostmanager.h>

ManagedCallback::ManagedCallback( Handle handle )
{
	m_handle = handle;
}

void ManagedCallback::Invoke()
{
	if ( m_handle != HANDLE_INVALID )
		Globals::m_hostManager->InvokeCallback( m_handle );
}