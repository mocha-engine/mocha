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

	// File exists so let's try to load it
	try
	{
		cvarFile >> cvarArchive;
	}
	catch ( nlohmann::json::parse_error& ex )
	{
		spdlog::error( "Couldn't parse cvars.json - skipping" );
		return;
	}

	for ( auto& [name, value] : cvarArchive.items() )
	{
		// If this cvar wasn't registered, we'll skip over the entry gracefully.
		if ( !Exists( name ) )
			continue;

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


	// Run a console command.
// The following formats are currently supported:
// - [convar name]: Output the current value for a console variable
// - [convar name] [value]: Update a console variable with a new value
void CVarSystem::Run( const char* command )
{
	std::string inputString = std::string( command );

	std::stringstream ss( inputString );

	std::string cvarName, cvarValue;
	ss >> cvarName >> cvarValue;

	std::stringstream valueStream( cvarValue );

	if ( cvarName == "list" )
	{
// This fails on libclang so we'll ignore it for now...
#ifndef __clang__
		// List all available cvars
		ForEach( [&]( CVarEntry& entry ) {
			spdlog::info( "- '{}': '{}'", entry.m_name, ToString( entry.m_name ) );
			spdlog::info( "\t{}", entry.m_description );
		} );
#endif
	}
	else if ( !Exists( cvarName ) )
	{
		spdlog::info( "{} is not a valid command or variable", cvarName );
	}
	else
	{
		CVarEntry& entry = GetEntry( cvarName );

		if ( entry.m_flags & CVarFlags::Command )
		{
			InvokeCommand( entry, {} );
		}
		else
		{
			if ( valueStream.str().size() > 0 )
			{
				FromString( entry, cvarValue );
			}
			else
			{
				cvarValue = ToString( entry );
				spdlog::info( "{} is '{}'", cvarName, cvarValue );
			}
		}
	}
}

bool CVarSystem::Exists( std::string name )
{
	return m_cvarEntries.find( GetHash( name ) ) != m_cvarEntries.end();
}

CVarEntry& CVarSystem::GetEntry( std::string name )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first

	size_t hash = GetHash( name );
	CVarEntry& entry = m_cvarEntries[hash];

	return entry;
}

void CVarSystem::RegisterCommand( std::string name, CVarFlags flags, std::string description, CCmdCallback callback )
{
	assert( callback != nullptr );

	CVarEntry entry = {};
	entry.m_name = name;
	entry.m_description = description;
	entry.m_flags = CVarFlags::Command | flags;
	entry.m_callback = callback;

	size_t hash = GetHash( name );
	m_cvarEntries[hash] = entry;
}

void CVarSystem::RegisterString( std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback )
{
	RegisterVariable<std::string>( name, value, flags, description, callback );
}

void CVarSystem::RegisterFloat( std::string name, float value, CVarFlags flags, std::string description, CVarCallback<float> callback )
{
	RegisterVariable<float>( name, value, flags, description, callback );
}

void CVarSystem::RegisterBool( std::string name, bool value, CVarFlags flags, std::string description, CVarCallback<bool> callback )
{
	RegisterVariable<bool>( name, value, flags, description, callback );
}


void CVarSystem::InvokeCommand( CVarEntry& entry, std::vector<std::string> arguments )
{
	assert( entry.m_flags & CVarFlags::Command ); // Should be a command

	auto callback = std::any_cast<CCmdCallback>( entry.m_callback );

	if ( callback )
	{
		callback( arguments );
	}
}

void CVarSystem::InvokeCommand( std::string name, std::vector<std::string> arguments )
{
	InvokeCommand( GetEntry( name ), arguments );
}


std::string CVarSystem::GetString( CVarEntry& entry )
{
	return GetVariable<std::string>( entry );
}

std::string CVarSystem::GetString( std::string name )
{
	return GetString( GetEntry( name ) );
}


float CVarSystem::GetFloat( CVarEntry& entry )
{
	return GetVariable<float>( entry );
}

float CVarSystem::GetFloat( std::string name )
{
	return GetFloat( GetEntry( name ) );
}


bool CVarSystem::GetBool( CVarEntry& entry )
{
	return GetVariable<bool>( entry );
}

bool CVarSystem::GetBool( std::string name )
{
	return GetBool( GetEntry( name ) );
}


void CVarSystem::SetString( CVarEntry& entry, std::string value )
{
	SetVariable<std::string>( entry, value );
}

void CVarSystem::SetString( std::string name, std::string value )
{
	SetString( GetEntry( name ), value );
}


void CVarSystem::SetFloat( CVarEntry& entry, float value )
{
	SetVariable<float>( entry, value );
}

void CVarSystem::SetFloat( std::string name, float value )
{
	SetFloat( GetEntry( name ), value );
}


void CVarSystem::SetBool( CVarEntry& entry, bool value )
{
	SetVariable<bool>( entry, value );
}

void CVarSystem::SetBool( std::string name, bool value )
{
	SetBool( GetEntry( name ), value );
}


void CVarSystem::FromString( CVarEntry& entry, std::string valueStr )
{
	std::stringstream valueStream( valueStr );

	auto& type = entry.m_value.type();

	if ( type == typeid( float ) )
	{
		float value;
		valueStream >> value;

		SetVariable<float>( entry, value );
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

		SetVariable<bool>( entry, value );
	}
	else if ( type == typeid( std::string ) )
	{
		SetVariable<std::string>( entry, valueStr );
	}
}

void CVarSystem::FromString( std::string name, std::string valueStr )
{
	FromString( GetEntry( name ), valueStr );
}


std::string CVarSystem::ToString( CVarEntry& entry )
{
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

std::string CVarSystem::ToString( std::string name )
{
	return ToString( GetEntry( name ) );
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

void CVarManager::Startup()
{
	CVarSystem::Instance().Startup();
}

void CVarManager::Shutdown()
{
	CVarSystem::Instance().Shutdown();
}

// ----------------------------------------
// Test CVars
// ----------------------------------------

FloatCVar cvartest_float( "cvartest.float", 0.0f, CVarFlags::None, "Yeah",
	[]( float oldValue, float newValue )
	{
		spdlog::trace( "cvartest.float changed! old {}, new {}", oldValue, newValue );
	}
);

CCmd cvartest_command( "cvartest.command", CVarFlags::None, "A test command",
	[]( std::vector<std::string> arguments ) {
		spdlog::trace( "cvartest.command has been invoked! Hooray" );
	}
);