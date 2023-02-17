#pragma once
#include <Misc/defs.h>
#include <Misc/globalvars.h>

class ManagedCallback
{
private:
	Handle m_handle = HANDLE_INVALID;

public:
	ManagedCallback() {}

	GENERATE_BINDINGS ManagedCallback( Handle handle );
	GENERATE_BINDINGS void Invoke();
};
