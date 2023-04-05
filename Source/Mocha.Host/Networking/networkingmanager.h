#pragma once
#include <Misc/subsystem.h>

class NetworkingManager : public ISubSystem
{
public:
	void Startup() override;
	void Shutdown() override;
};
