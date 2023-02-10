#pragma once
#include <root.h>

class ServerRoot : public Root
{
private:
	ServerRoot() { m_executingRealm = REALM_SERVER; }

protected:
	bool GetQuitRequested() override;

public:
	inline static ServerRoot& GetInstance()
	{
		static ServerRoot instance;
		return instance;
	}
};