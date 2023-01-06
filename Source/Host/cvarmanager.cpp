#include "cvarmanager.h"

#include <sstream>

size_t CVarSystem::GetHash( std::string string )
{
	return std::hash<std::string>{}( string );
}

void CVarSystem::Startup()
{
	// Load all archive cvars from disk
	nlohmann::json cvarArchive;

	std::ifstream cvarFile( "cvars.json" );

	// Does the cvars file exist?
	if ( !cvarFile.good() )
	{
		spdlog::info( "No cvars.json file was found -- using default values for all archived cvars" );
		return;
	}

	// File exists so let's load it
	cvarFile >> cvarArchive;

	for ( auto& [name, value] : cvarArchive.items() )
	{
		assert( Exists( name ) ); // Doesn't exist! Register it first

		size_t hash = GetHash( name );
		CVarEntry& entry = m_cvarEntries[hash];

		if ( entry.m_flags & CVarFlags::Archive )
			FromString( name, value );
	}
}

void CVarSystem::Shutdown()
{
	// Save all archive cvars to disk
	nlohmann::json cvarArchive;

	for ( auto& entry : m_cvarEntries )
	{
		if ( entry.second.m_flags & CVarFlags::Archive )
			cvarArchive[entry.second.m_name] = ToString( entry.second.m_name );
	}

	std::ofstream cvarFile( "cvars.json" );
	cvarFile << std::setw( 4 ) << cvarArchive << std::endl;
}

void CVarSystem::RegisterString( std::string name, std::string value, CVarFlags flags, std::string description )
{
	Register<std::string>( name, value, flags, description );
}

void CVarSystem::RegisterFloat( std::string name, float value, CVarFlags flags, std::string description )
{
	Register<float>( name, value, flags, description );
}

void CVarSystem::RegisterBool( std::string name, bool value, CVarFlags flags, std::string description )
{
	Register<bool>( name, value, flags, description );
}

std::string CVarSystem::GetString( std::string name )
{
	return Get<std::string>( name );
}

float CVarSystem::GetFloat( std::string name )
{
	return Get<float>( name );
}

bool CVarSystem::GetBool( std::string name )
{
	return Get<bool>( name );
}

void CVarSystem::ForEach( std::function<void( CVarEntry& entry )> func )
{
	for ( auto& entry : m_cvarEntries )
	{
		func( entry.second );
	}
}

void CVarSystem::ForEach( std::string filter, std::function<void( CVarEntry& entry )> func )
{
	std::vector<CVarEntry> matchingEntries = {};

	for ( auto& item : m_cvarEntries )
	{
		if ( item.second.m_name.find( filter ) == std::string::npos )
			continue;

		matchingEntries.push_back( item.second );
	}

	for ( auto& entry : matchingEntries )
	{
		func( entry );
	}
}

bool CVarSystem::Exists( std::string name )
{
	return m_cvarEntries.find( GetHash( name ) ) != m_cvarEntries.end();
}

void CVarSystem::FromString( std::string name, std::string valueStr )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first

	std::stringstream valueStream( valueStr );
	size_t hash = GetHash( name );
	CVarEntry& entry = m_cvarEntries[hash];

	auto& type = entry.m_value.type();

	if ( type == typeid( float ) )
	{
		float value;
		valueStream >> value;

		Set<float>( entry.m_name, value );
	}
	else if ( type == typeid( bool ) )
	{
		bool value;

		if ( valueStr == "true" || valueStr == "1" || valueStr == "yes" )
			value = true;
		else if ( valueStr == "false" || valueStr == "0" || valueStr == "no" )
			value = false;
		else
			assert( false ); // Invalid bool value

		Set<bool>( entry.m_name, value );
	}
	else if ( type == typeid( std::string ) )
	{
		Set<std::string>( entry.m_name, valueStr );
	}
}

std::string CVarSystem::ToString( std::string name )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first

	size_t hash = GetHash( name );
	CVarEntry& entry = m_cvarEntries[hash];

	const std::type_info& type = entry.m_value.type();
	std::string valueStr;

	if ( type == typeid( std::string ) )
		valueStr = std::any_cast<std::string>( entry.m_value );
	else if ( type == typeid( float ) )
		valueStr = std::to_string( std::any_cast<float>( entry.m_value ) );
	else if ( type == typeid( bool ) )
		valueStr = std::any_cast<bool>( entry.m_value ) ? "true" : "false";

	return valueStr;
}

void CVarSystem::SetString( std::string name, std::string value )
{
	Set<std::string>( name, value );
}

void CVarSystem::SetFloat( std::string name, float value )
{
	Set<float>( name, value );
}

void CVarSystem::SetBool( std::string name, bool value )
{
	Set<bool>( name, value );
}

void CVarManager::Startup()
{
	CVarSystem::Instance().Startup();
}

void CVarManager::Shutdown()
{
	CVarSystem::Instance().Shutdown();
}
