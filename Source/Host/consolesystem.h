#pragma once

#include <cvarmanager.h>
#include <defs.h>
#include <globalvars.h>
#include <sstream>
#include <string>

namespace ConsoleSystem
{
	GENERATE_BINDINGS inline void Run( const char* command )
	{
		CVarSystem::Instance().Run( command );
	}

	// TODO: Not until all memory leak concerns are addressed
/*
	GENERATE_BINDINGS inline const char* GetString( const char* name )
	{
		return CVarSystem::Instance().GetString( name ).c_str();
	}
*/

	GENERATE_BINDINGS inline float GetFloat( const char* name )
	{
		return CVarSystem::Instance().GetFloat( name );
	}

	GENERATE_BINDINGS inline bool GetBool( const char* name )
	{
		return CVarSystem::Instance().GetBool( name );
	}

	GENERATE_BINDINGS inline void SetString( const char* name, const char* value )
	{
		CVarSystem::Instance().SetString( name, value );
	}

	GENERATE_BINDINGS inline void SetFloat( const char* name, float value )
	{
		CVarSystem::Instance().SetFloat( name, value );
	}

	GENERATE_BINDINGS inline void SetBool( const char* name, bool value )
	{
		CVarSystem::Instance().SetBool( name, value );
	}

	// TODO: Not until all memory leak concerns are addressed
/*
	GENERATE_BINDINGS inline const char* ToString( const char* name )
	{
		
	}
*/

	GENERATE_BINDINGS inline void FromString( const char* name, const char* valueStr )
	{
		CVarSystem::Instance().FromString( name, valueStr );
	}
} // namespace ConsoleSystem