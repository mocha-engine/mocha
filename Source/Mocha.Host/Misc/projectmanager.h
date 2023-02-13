#pragma once

#include <Misc/defs.h>
#include <Misc/projectmanifest.h>
#include <Misc/subsystem.h>

class ProjectManager : public ISubSystem
{
private:
	Project m_project;

public:
	virtual void Startup() override;
	virtual void Shutdown() override;

	Project GetProject() { return m_project; }
};
