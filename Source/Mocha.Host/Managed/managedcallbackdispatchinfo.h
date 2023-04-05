#pragma once
#include <Misc/defs.h>

struct ManagedCallbackDispatchInfo
{
	Handle handle = HANDLE_INVALID;

	int argsSize;
	void* args;
};
