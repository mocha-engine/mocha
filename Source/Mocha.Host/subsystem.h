#pragma once

class Root;

class ISubSystem
{
protected:
	Root* m_parent;

public:
	ISubSystem( Root* parent )
	    : m_parent( parent )
	{
	}

	virtual void Startup() = 0;
	virtual void Shutdown() = 0;
};
