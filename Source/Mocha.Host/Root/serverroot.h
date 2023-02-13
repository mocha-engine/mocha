#pragma once
#include <Root/root.h>

class ServerRoot : public Root
{
protected:
	bool GetQuitRequested() override;

public:
	ServerRoot() { Globals::m_executingRealm = REALM_SERVER; }
};