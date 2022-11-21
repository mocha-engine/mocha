#pragma once

class ISubSystem
{
public:
	virtual void Startup() = 0;
	virtual void Shutdown() = 0;
};
