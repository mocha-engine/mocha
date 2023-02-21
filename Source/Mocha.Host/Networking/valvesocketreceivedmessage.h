#pragma once

struct ValveSocketReceivedMessage
{
	void* connectionHandle;

	int size;
	void* data;
};