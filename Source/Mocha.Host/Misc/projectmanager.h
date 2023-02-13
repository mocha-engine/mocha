#pragma once

#include <Misc/defs.h>
#include <Misc/projectmanifest.h>
#include <Misc/subsystem.h>

class ProjectManager : public ISubSystem
{
private:
	Project m_project;

public:
	ProjectManager( Root* parent )
	    : ISubSystem( parent )
	{
	}

	virtual void Startup() override;
	virtual void Shutdown() override;

	Project GetProject() { return m_project; }
};
