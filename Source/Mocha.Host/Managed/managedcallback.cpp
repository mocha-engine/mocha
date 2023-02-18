#include "managedcallback.h"

#include <Managed/hostmanager.h>

ManagedCallback::ManagedCallback( Handle handle )
{
	m_handle = handle;
}

void ManagedCallback::Invoke()
{
	Invoke( nullptr );
}

void ManagedCallback::Invoke( void* args )
{
	if ( m_handle != HANDLE_INVALID )
		Globals::m_hostManager->InvokeCallback( m_handle, args );
}
