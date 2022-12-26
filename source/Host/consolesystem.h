#pragma once

#include <cvarmanager.h>
#include <globalvars.h>
#include <sstream>
#include <string>

//@InteropGen generate class
namespace ConsoleSystem
{
	inline void Run( const char* command )
	{
		std::string inputString = std::string( command );

		std::stringstream ss( inputString );

		std::string cvarName, cvarValue;
		ss >> cvarName >> cvarValue;

		std::stringstream valueStream( cvarValue );

		if ( !CVarManager::Instance().Exists( cvarName ) )
		{
			spdlog::info( "{} is not a valid command or variable", cvarName );
		}
		else
		{
			if ( valueStream.str().size() > 0 )
			{
				CVarManager::Instance().FromString( cvarName, cvarValue );
			}
			else
			{
				cvarValue = CVarManager::Instance().ToString( cvarName );
				spdlog::info( "{} is '{}'", cvarName, cvarValue );
			}
		}
	}
} // namespace ConsoleSystem