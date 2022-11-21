#pragma once

class ISubSystem
{
public:
	virtual void StartUp() = 0;
	virtual void ShutDown() = 0;
};
