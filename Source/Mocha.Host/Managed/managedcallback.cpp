#include "managedcallback.h"

#include <Managed/hostmanager.h>

ManagedCallback::ManagedCallback( Handle handle )
{
	m_handle = handle;
}

void ManagedCallback::Invoke()
{
	InternalInvoke( 0, nullptr );
}

void ManagedCallback::Invoke( void* args )
{
	InternalInvoke( 1, args );
}

void ManagedCallback::InternalInvoke( int argsCount, void* args )
{
	if ( m_handle != HANDLE_INVALID )
		Globals::m_hostManager->InvokeCallback( m_handle, argsCount, args );
}
