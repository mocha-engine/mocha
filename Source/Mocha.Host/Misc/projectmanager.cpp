#include "ProjectManager.h"

#include <Misc/cvarmanager.h>
#include <Misc/globalvars.h>

void ProjectManager::Startup()
{
	// If the command-line has passed a project, use that instead.
	if ( Globals::m_activeProjectPath )
		EngineProperties::LoadedProject.SetValue( Globals::m_activeProjectPath );
	else
		Globals::m_activeProjectPath = EngineProperties::LoadedProject.GetValue().data();

	// Load project from json
	m_project = Project( EngineProperties::LoadedProject );

	// Load all archive cvars from disk
	nlohmann::json projectManifest;

	std::ifstream projectFile( EngineProperties::LoadedProject );

	// Does the file exist?
	if ( !projectFile.good() )
	{
		ErrorMessage( "Missing project file" );
		abort();
	}

	// File exists so let's try to load it
	try
	{
		projectFile >> projectManifest;
		ProjectManifest::FromJson( projectManifest, m_project );

		// Get the directory for EngineProperties::LoadedProject so that we
		// can use it to get absolute paths for resources
		std::filesystem::path projectPath = EngineProperties::LoadedProject.GetValue();
		std::string parentDirectory = projectPath.parent_path().string();

		ProjectManifest::NormalizePaths( parentDirectory, m_project );
	}
	catch ( nlohmann::json::parse_error& ex )
	{
		ErrorMessage( "Invalid project file" );
		abort();
	}

	spdlog::info( "Loaded project" );
}

void ProjectManager::Shutdown() {}
