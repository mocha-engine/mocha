#pragma once
#include <root.h>

class ServerRoot : public Root
{
protected:
	bool GetQuitRequested() override;

public:
	ServerRoot() { m_executingRealm = REALM_SERVER; }
};