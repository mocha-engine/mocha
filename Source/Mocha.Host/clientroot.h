#pragma once
#include <root.h>

class ClientRoot : public Root
{
protected:
	bool GetQuitRequested() override;

public:
	ClientRoot() { m_executingRealm = REALM_CLIENT; }
};