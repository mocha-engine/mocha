#pragma once
#include <subsystem.h>

class Root : ISubSystem
{
public:
	inline static Root& GetInstance()
	{
		static Root instance;
		return instance;
	}

	void StartUp();
	void ShutDown();
};
