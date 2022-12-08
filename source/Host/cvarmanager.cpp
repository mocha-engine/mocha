#include "cvarmanager.h"

#include <sstream>

size_t CVarManager::GetHash( std::string string )
{
	return std::hash<std::string>{}( string );
}

void CVarManager::ForEach( std::function<void( CVarEntry& entry )> func )
{
	for ( auto& entry : m_cvarEntries )
	{
		func( entry.second );
	}
}

void CVarManager::FromString( std::string name, std::string valueStr )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first
	
	std::stringstream valueStream( valueStr );

	if ( valueStream.str().find_first_of( "." ) != std::string::npos )
	{
		float value;
		valueStream >> value;

		CVarManager::Instance().SetFloat( name, value );
	}
	else if ( valueStream.str() == "true" || valueStream.str() == "false" )
	{
		bool value;
		valueStream >> std::boolalpha >> value;

		// TODO
	}
	else if ( valueStream.str().size() > 0 )
	{
		CVarManager::Instance().SetString( name, valueStr );
	}
}

std::string CVarManager::ToString( std::string name )
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

	return valueStr;
}
