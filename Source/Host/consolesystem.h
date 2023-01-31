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

	GENERATE_BINDINGS inline bool Exists( const char* name )
	{
		return CVarSystem::Instance().Exists( name );
	}

	GENERATE_BINDINGS inline void RegisterCommand( const char* name, CVarFlags flags, const char* description )
	{
		CVarSystem::Instance().RegisterCommand( name, ( CVarFlags )( CVarFlags::Managed | flags ), description, nullptr );
	}

	GENERATE_BINDINGS inline void RegisterString( const char* name, const char* value, CVarFlags flags, const char* description )
	{
		CVarSystem::Instance().RegisterString( name, value, ( CVarFlags )( CVarFlags::Managed | flags ), description, nullptr );
	}

	GENERATE_BINDINGS inline void RegisterFloat( const char* name, float value, CVarFlags flags, const char* description )
	{
		CVarSystem::Instance().RegisterFloat( name, value, ( CVarFlags )( CVarFlags::Managed | flags ), description, nullptr );
	}

	GENERATE_BINDINGS inline void RegisterBool( const char* name, bool value, CVarFlags flags, const char* description )
	{
		CVarSystem::Instance().RegisterBool( name, value, ( CVarFlags )( CVarFlags::Managed | flags ), description, nullptr );
	}

	GENERATE_BINDINGS inline void Remove( const char* name )
	{
		CVarSystem::Instance().Remove( name );
	}

	GENERATE_BINDINGS inline CVarFlags GetFlags( const char* name )
	{
		return CVarSystem::Instance().GetFlags( name );
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