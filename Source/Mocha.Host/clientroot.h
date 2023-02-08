#pragma once
#include <root.h>

class ClientRoot : public Root
{
protected:
	bool GetQuitRequested() override;

public:
	inline static ClientRoot& GetInstance()
	{
		static ClientRoot instance;
		return instance;
	}
};