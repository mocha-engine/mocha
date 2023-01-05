#pragma once

#include <cvarmanager.h>
#include <defs.h>
#include <globalvars.h>
#include <sstream>
#include <string>

namespace ConsoleSystem
{
	// Run a console command.
	// The following formats are currently supported:
	// - [convar name]: Output the current value for a console variable
	// - [convar name] [value]: Update a console variable with a new value
	GENERATE_BINDINGS inline void Run( const char* command )
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
			CVarSystem::Instance().ForEach( [&]( CVarEntry& entry ) {
				spdlog::info( "- '{}': '{}'", entry.m_name, CVarSystem::Instance().ToString( entry.m_name ) );
				spdlog::info( "\t{}", entry.m_description );
			} );
#endif
		}
		else if ( !CVarSystem::Instance().Exists( cvarName ) )
		{
			spdlog::info( "{} is not a valid command or variable", cvarName );
		}
		else
		{
			if ( valueStream.str().size() > 0 )
			{
				CVarSystem::Instance().FromString( cvarName, cvarValue );
			}
			else
			{
				cvarValue = CVarSystem::Instance().ToString( cvarName );
				spdlog::info( "{} is '{}'", cvarName, cvarValue );
			}
		}
	}
} // namespace ConsoleSystem