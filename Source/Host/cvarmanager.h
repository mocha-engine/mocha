#pragma once
#include <any>
#include <cstdint>
#include <fstream>
#include <globalvars.h>
#include <memory>
#include <nlohmann/json.hpp>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>
#include <unordered_map>

// ----------------------------------------
// Core CVar functionality
// ----------------------------------------

template <typename T>
using CVarCallback = std::function<void( T, T )>;

using CCmdCallback = std::function<void( std::vector<std::string> )>;

enum CVarFlags : uint32_t
{
	None = 0,

	// If this isn't present, it's inherently assumed to be a variable
	Command = 1 << 0,

	// Save this convar to cvars.json
	Archive = 1 << 1,

	// TODO
	Cheat = 1 << 2,

	// TODO
	Temp = 1 << 3,

	// TODO: Networked variables server -> client
	Replicated = 1 << 4,

	// TODO: CVars created by the game, hotload these?
	Game = 1 << 5
};

struct CVarEntry
{
	std::string m_name;
	std::string m_description;

	uint32_t m_flags;

	std::any m_value;
	std::any m_callback;
};

class CVarManager : ISubSystem
{
public:
	void Startup() override;
	void Shutdown() override;
};

class CVarSystem
{
private:
	std::unordered_map<std::size_t, CVarEntry> m_cvarEntries;
	size_t GetHash( std::string string );

	template <typename T>
	void RegisterVariable( std::string name, T value, CVarFlags flags, std::string description, CVarCallback<T> callback );

	template <typename T>
	T GetVariable( std::string name );

	template <typename T>
	void SetVariable( std::string name, T value );

	CVarEntry& GetEntry( std::string name );

public:

	//
	// CVarSystem is a singleton because it needs creating *as soon as* it's referenced
	// and not after.
	//
	static CVarSystem& Instance()
	{
		static CVarSystem instance;
		return instance;
	}

	void Startup();
	void Shutdown();

	void Run( const char* command );

	/// <summary>
	/// Check if a specific convar exists
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	bool Exists( std::string name );

	void RegisterCommand( std::string name, CVarFlags flags, std::string description, CCmdCallback callback );

	void RegisterString( std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback );
	void RegisterFloat( std::string name, float value, CVarFlags flags, std::string description, CVarCallback<float> callback );
	void RegisterBool( std::string name, bool value, CVarFlags flags, std::string description, CVarCallback<bool> callback );

	void InvokeCommand( std::string name, std::vector<std::string> arguments );

	std::string GetString( std::string name );
	float GetFloat( std::string name );
	bool GetBool( std::string name );

	void SetString( std::string name, std::string value );
	void SetFloat( std::string name, float value );
	void SetBool( std::string name, bool value );

	void ForEach( std::function<void( CVarEntry& entry )> func );
	void ForEach( std::string filter, std::function<void( CVarEntry& entry )> func );

	inline static float AsFloat( std::string& argument ) { return std::strtof( argument.c_str(), nullptr ); }
	inline static bool AsBool( std::string& argument ) { return argument == "true"; }

	void FromString( std::string name, std::string valueStr );
	std::string ToString( std::string name );
};

template <typename T>
inline void CVarSystem::RegisterVariable( std::string name, T value, CVarFlags flags, std::string description, CVarCallback<T> callback )
{
	CVarEntry entry = {};
	entry.m_name = name;
	entry.m_description = description;
	entry.m_flags = flags;
	entry.m_value = value;
	entry.m_callback = callback;

	size_t hash = GetHash( name );
	m_cvarEntries[hash] = entry;
}

template <typename T>
inline T CVarSystem::GetVariable( std::string name )
{
	CVarEntry& entry = GetEntry( name );

	assert( !( entry.m_flags & CVarFlags::Command ) ); // Should be a variable

	return std::any_cast<T>( entry.m_value );
}

template <typename T>
inline void CVarSystem::SetVariable( std::string name, T value )
{
	CVarEntry& entry = GetEntry( name );

	assert( !( entry.m_flags & CVarFlags::Command ) ); // Should be a variable

	T lastValue = std::any_cast<T>( entry.m_value );

	entry.m_value = value;

	auto callback = std::any_cast<CVarCallback<T>>( entry.m_callback );

	if ( callback )
	{
		callback( lastValue, value );
	}

	spdlog::info( "{} was set to '{}'.", entry.m_name, value );
}

// ----------------------------------------
// Native CVar interface
// ----------------------------------------

class CVarParameter
{
protected:
	std::string m_name;

public:
	friend class CVarSystem;
};

class StringCVar : CVarParameter
{
public:
	StringCVar( std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback )
	{
		m_name = name;

		CVarSystem::Instance().RegisterString( name, value, flags, description, callback );
	}

	StringCVar( std::string name, std::string value, CVarFlags flags, std::string description )
	    : StringCVar( name, value, flags, description, nullptr )
	{
	
	}

	std::string GetValue() { return CVarSystem::Instance().GetString( m_name ); }
	void SetValue( std::string value ) { CVarSystem::Instance().SetString( m_name, value ); }
	
	operator std::string() { return GetValue(); }
};

class FloatCVar : CVarParameter
{
public:
	FloatCVar( std::string name, float value, CVarFlags flags, std::string description, CVarCallback<float> callback )
	{
		m_name = name;

		CVarSystem::Instance().RegisterFloat( name, value, flags, description, callback );
	}

	FloatCVar( std::string name, float value, CVarFlags flags, std::string description )
	    : FloatCVar( name, value, flags, description, nullptr )
	{

	}

	float GetValue() { return CVarSystem::Instance().GetFloat( m_name ); }
	void SetValue( float value ) { CVarSystem::Instance().SetFloat( m_name, value ); }
	
	operator float() { return GetValue(); }
};

class BoolCVar : CVarParameter
{
public:
	BoolCVar( std::string name, bool value, CVarFlags flags, std::string description, CVarCallback<bool> callback )
	{
		m_name = name;

		CVarSystem::Instance().RegisterBool( name, value, flags, description, callback );
	}

	BoolCVar( std::string name, bool value, CVarFlags flags, std::string description )
	    : BoolCVar( name, value, flags, description, nullptr )
	{

	}

	bool GetValue() { return CVarSystem::Instance().GetBool( m_name ); }
	void SetValue( bool value ) { CVarSystem::Instance().SetBool( m_name, value ); }

	operator bool() { return GetValue(); };
};

class CCmd : CVarParameter
{
public:
	CCmd( std::string name, CVarFlags flags, std::string description, CCmdCallback callback )
	{
		m_name = name;

		CVarSystem::Instance().RegisterCommand( name, flags, description, callback );
	}

	//
	// You can invoke like this, but honestly, just define a separate function.
	// This is not going to be as clean as C#.
	//

	void Invoke( std::vector<std::string> arguments )
	{
		CVarSystem::Instance().InvokeCommand( m_name, arguments );
	}

	void operator()( std::vector<std::string> arguments ) { Invoke( arguments ); }
};