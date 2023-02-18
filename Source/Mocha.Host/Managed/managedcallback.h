#pragma once
#include <Misc/defs.h>
#include <Misc/globalvars.h>

class ManagedCallback
{
private:
	Handle m_handle = HANDLE_INVALID;

public:
	ManagedCallback() {}

	ManagedCallback( Handle handle );

	void Invoke();
	void Invoke( void* args );
};