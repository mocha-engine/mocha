#pragma once
#include <root.h>

class ServerRoot : public Root
{
protected:
	bool GetQuitRequested() override;

public:
	inline static ServerRoot& GetInstance()
	{
		static ServerRoot instance;
		return instance;
	}
};
