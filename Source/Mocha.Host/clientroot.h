#pragma once
#include <root.h>

class ClientRoot : public Root
{
private:
	ClientRoot() { m_executingRealm = REALM_CLIENT; }

protected:
	bool GetQuitRequested() override;

public:
	inline static ClientRoot& GetInstance()
	{
		static ClientRoot instance;
		return instance;
	}
};