#include "ProjectManager.h"

void ProjectManager::Startup()
{
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
		ProjectManifest::from_json( projectManifest, m_project );
	}
	catch ( nlohmann::json::parse_error& ex )
	{
		ErrorMessage( "Invalid project file" );
		abort();
	}

	spdlog::info( "Loaded project" );
}

void ProjectManager::Shutdown() {}
