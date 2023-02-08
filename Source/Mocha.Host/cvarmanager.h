#pragma once
#include <any>
#include <cstdint>
#include <defs.h>
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

struct CVarManagedCmdDispatchInfo
{
	const char* name;
	void* data;
	int size;
};

template <typename T>
struct CVarManagedVarDispatchInfo
{
	const char* name;
	T oldValue;
	T newValue;
};

enum CVarFlags : int32_t
{
	None = 0,

	// If this isn't present, it's inherently assumed to be a variable
	Command = 1 << 0,

	// If this is present, it lives in managed space
	Managed = 1 << 1,

	// This cvar was created by the game, it should be wiped on hotload
	Game = 1 << 2,

	// Save this convar to cvars.json
	Archive = 1 << 3,

	// TODO
	Cheat = 1 << 4,

	// TODO
	Temp = 1 << 5,

	// TODO: Networked variables server -> client
	Replicated = 1 << 6,
};

struct CVarEntry
{
private:
	template <typename T>
	T GetValue();

	template <typename T>
	void SetValue( T value );

public:
	std::string m_name;
	std::string m_description;

	int32_t m_flags;

	std::any m_value;
	std::any m_callback;

	inline bool IsCommand() const { return m_flags & CVarFlags::Command; }
	inline bool IsManaged() const { return m_flags & CVarFlags::Managed; }

	// Commands

	void InvokeCommand( std::vector<std::string> arguments );

	// Variables

	std::string GetString();
	float GetFloat();
	bool GetBool();
	int GetInt();

	void SetString( std::string value );
	void SetFloat( float value );
	void SetBool( bool value );
	void SetInt( int value );

	std::string ToString();
	void FromString( std::string valueStr );
};

class CVarManager : ISubSystem
{
public:
	CVarManager( Root* parent )
	    : ISubSystem( parent )
	{
	}

	void Startup() override;
	void Shutdown() override;

	GENERATE_BINDINGS void Run( const char* input );
};

class CVarSystem
{
private:
	std::unordered_map<std::size_t, CVarEntry> m_cvarEntries;
	size_t GetHash( std::string string );

	template <typename T>
	void RegisterVariable( std::string name, T value, CVarFlags flags, std::string description, CVarCallback<T> callback );

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

	/// <summary>
	/// Get command arguments from a statement, for use with GetStatements.
	/// Ignores comments, starting with "//".
	/// </summary>
	/// <param name="statement">The statement to get arguments from</param>
	/// <param name="cursor">Where the user's cursor currently is within the statement string</param>
	/// <param name="cursorIndex">Returns which argument the cursor is within</param>
	static std::vector<std::string> GetStatementArguments( std::string_view statement, size_t cursor, size_t& cursorIndex );

	/// <summary>
	/// Get the statements from a string, with each statement separated either by ";" or a newline.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="cursor">Where the user's cursor currently is within the input string</param>
	/// <param name="cursorIndex">Returns which statement the cursor is within</param>
	static std::vector<std::string_view> GetStatements( const std::string& input, size_t cursor, size_t& cursorIndex );

	// Variants for uses without a text cursor
	static std::vector<std::string> GetStatementArguments( std::string_view statement );
	static std::vector<std::string_view> GetStatements( const std::string& input );

	/// <summary>
	/// Run statements in the console
	/// </summary>
	/// <param name="input"></param>
	void Run( const char* input );

	/// <summary>
	/// Run statements from a .cfg file
	/// </summary>
	/// <param name="fileName"></param>
	void RunFile( std::string fileName );

	/// <summary>
	/// Check if a specific convar exists
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	bool Exists( std::string name );

	CVarEntry& GetEntry( std::string name );

	void RegisterCommand( std::string name, CVarFlags flags, std::string description, CCmdCallback callback );

	void RegisterString(
	    std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback );
	void RegisterFloat( std::string name, float value, CVarFlags flags, std::string description, CVarCallback<float> callback );
	void RegisterBool( std::string name, bool value, CVarFlags flags, std::string description, CVarCallback<bool> callback );
	void RegisterInt( std::string name, int value, CVarFlags flags, std::string description, CVarCallback<int> callback );

	void Remove( std::string name );

	void InvokeCommand( std::string name, std::vector<std::string> arguments );

	CVarFlags GetFlags( std::string name );

	std::string GetString( std::string name );
	float GetFloat( std::string name );
	bool GetBool( std::string name );
	int GetInt( std::string name );

	void SetString( std::string name, std::string value );
	void SetFloat( std::string name, float value );
	void SetBool( std::string name, bool value );
	void SetInt( std::string name, int value );

	std::string ToString( std::string name );
	void FromString( std::string name, std::string valueStr );

	void ForEach( std::function<void( CVarEntry& entry )> func );
	void ForEach( std::string filter, std::function<void( CVarEntry& entry )> func );

	inline static float AsFloat( std::string& argument ) { return std::strtof( argument.c_str(), nullptr ); }
	inline static bool AsBool( std::string& argument ) { return argument == "true"; }
};

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
	StringCVar(
	    std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback )
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

class IntCVar : CVarParameter
{
public:
	IntCVar( std::string name, int value, CVarFlags flags, std::string description, CVarCallback<int> callback )
	{
		m_name = name;

		CVarSystem::Instance().RegisterInt( name, value, flags, description, callback );
	}

	IntCVar( std::string name, int value, CVarFlags flags, std::string description )
	    : IntCVar( name, value, flags, description, nullptr )
	{
	}

	int GetValue() { return CVarSystem::Instance().GetInt( m_name ); }
	void SetValue( int value ) { CVarSystem::Instance().SetInt( m_name, value ); }

	operator int() { return GetValue(); };
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

	void Invoke( std::vector<std::string> arguments ) { CVarSystem::Instance().InvokeCommand( m_name, arguments ); }

	void operator()( std::vector<std::string> arguments ) { Invoke( arguments ); }
};