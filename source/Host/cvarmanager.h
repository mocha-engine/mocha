#pragma once
#include <any>
#include <globalvars.h>
#include <memory>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>
#include <unordered_map>

enum CVarFlags
{
	None = 0,
	Archive = 1 << 0,
	UserInfo = 1 << 1,
	ServerInfo = 1 << 2,
	Init = 1 << 3,
	Latch = 1 << 4,
	ReadOnly = 1 << 5,
	UserCreated = 1 << 6,
	Temp = 1 << 7,
	Cheat = 1 << 8,
	NoRestart = 1 << 9
};

struct CVarEntry
{
	std::string m_name;
	std::string m_description;

	std::any m_value;
};

class CVarParameter
{
protected:
	std::string m_name;

public:
	friend class CVarManager;
};

class CVarManager
{
private:
	std::unordered_map<std::size_t, CVarEntry> m_cvarEntries;
	size_t GetHash( std::string string );

	template <typename T>
	void Register( std::string name, T value, CVarFlags flags, std::string description );

	template <typename T>
	T Get( std::string name );

	template <typename T>
	void Set( std::string name, T value );

public:
	friend class StringCVar;

	static CVarManager& Instance()
	{
		static CVarManager instance;
		return instance;
	}

	void RegisterString( std::string name, std::string value, CVarFlags flags, std::string description )
	{
		Register<std::string>( name, value, flags, description );
	}

	void RegisterFloat( std::string name, float value, CVarFlags flags, std::string description )
	{
		Register<float>( name, value, flags, description );
	}

	std::string GetString( std::string name ) { return Get<std::string>( name ); }
	float GetFloat( std::string name ) { return Get<float>( name ); }

	void FromString( std::string name, std::string valueStr );
	std::string ToString( std::string name );

	void SetString( std::string name, std::string value ) { Set<std::string>( name, value ); }
	void SetFloat( std::string name, float value ) { Set<float>( name, value ); }

	void ForEach( std::function<void( CVarEntry& entry )> func );
	void ForEach( std::string filter, std::function<void( CVarEntry& entry )> func );

	bool Exists( std::string name ) { return m_cvarEntries.find( GetHash( name ) ) != m_cvarEntries.end(); }
};

class StringCVar : CVarParameter
{
public:
	StringCVar( std::string name, std::string value, CVarFlags flags, std::string description )
	{
		m_name = name;

		CVarManager::Instance().RegisterString( name, value, flags, description );
	}

	std::string GetValue() { return CVarManager::Instance().GetString( m_name ); }

	void SetValue( std::string value ) { CVarManager::Instance().SetString( m_name, value ); }
};

class FloatCVar : CVarParameter
{
public:
	FloatCVar( std::string name, float value, CVarFlags flags, std::string description )
	{
		m_name = name;

		CVarManager::Instance().RegisterFloat( name, value, flags, description );
	}

	float GetValue() { return CVarManager::Instance().GetFloat( m_name ); }

	void SetValue( float value ) { CVarManager::Instance().SetFloat( m_name, value ); }
};

template <typename T>
inline void CVarManager::Register( std::string name, T value, CVarFlags flags, std::string description )
{
	CVarEntry entry = {};
	entry.m_name = name;
	entry.m_description = description;

	entry.m_value = value;

	size_t hash = GetHash( name );
	m_cvarEntries[hash] = entry;
}

template <typename T>
inline T CVarManager::Get( std::string name )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first
	
	size_t hash = GetHash( name );
	CVarEntry& entry = m_cvarEntries[hash];

	return std::any_cast<T>( entry.m_value );
}

template <typename T>
inline void CVarManager::Set( std::string name, T value )
{
	assert( Exists( name ) ); // Doesn't exist! Register it first

	size_t hash = GetHash( name );
	CVarEntry& entry = m_cvarEntries[hash];

	entry.m_value = value;

	spdlog::info( "{} was set to '{}'.", entry.m_name, value );
}