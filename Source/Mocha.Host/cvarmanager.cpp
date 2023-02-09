#include "cvarmanager.h"

#include <hostmanager.h>
#include <sstream>

size_t CVarSystem::GetHash( std::string string )
{
	std::transform( string.begin(), string.end(), string.begin(), []( unsigned char c ) { return std::tolower( c ); } );
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
			entry.FromString( value );
	}

	// Register commands
	RegisterCommand( "cvars.run", CVarFlags::Command, "Run commands from a file", [&]( std::vector<std::string> args ) {
		if ( args.size() != 1 )
		{
			spdlog::error( "Invalid number of arguments" );
			return;
		}

		std::string fileName = args[0];

		RunFile( fileName );
	} );
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

#pragma region Parsing

// I hate parsing text in C/C++. This is super messy.
// It works though.

std::vector<std::string> CVarSystem::GetStatementArguments( std::string_view statement, size_t cursor, size_t& cursorIndex )
{
	std::vector<std::string> arguments;

	auto begin = statement.begin();
	auto end = statement.end();

	bool quoted = false;
	bool commented = false;
	std::string_view::const_iterator argumentStart = begin;
	char lastChar = ' ';

	cursorIndex = -1;
	bool cursorReached = false;

	auto setCursor = [&]( auto argStart, auto argEnd ) {
		size_t argStartIndex = argStart - begin;
		size_t argEndIndex = argEnd - begin;
		bool set = !cursorReached && ( argStartIndex <= cursor && cursor < argEndIndex );
		if ( set )
		{
			cursorIndex = arguments.size() - 1;
			cursorReached = true;
		}
	};

	for ( auto curChar = begin; curChar != end; curChar++ )
	{
		size_t curCharIndex = curChar - begin;
		auto nextChar = curChar + 1;

		if ( quoted )
		{
			if ( *curChar == '"' )
			{
				quoted = !quoted;
				arguments.emplace_back( argumentStart + 1, curChar );
				setCursor( argumentStart, curChar );
			}
			else if ( nextChar == end )
			{
				arguments.emplace_back( argumentStart + 1, end );
				setCursor( argumentStart, end );
			}
		}
		else
		{
			switch ( *curChar )
			{
			case '"':
				if ( lastChar != ' ' )
				{
					arguments.emplace_back( argumentStart, curChar );
					setCursor( argumentStart, curChar );
				}

				quoted = !quoted;
				argumentStart = curChar;
				break;

			case ' ':
				switch ( lastChar )
				{
				case ' ':
				case '"':
					break;
				default:
					arguments.emplace_back( argumentStart, curChar );
					setCursor( argumentStart, curChar );
					break;
				}
				break;

			case '/':
				if ( nextChar != end )
				{
					if ( *nextChar == '/' )
					{
						switch ( lastChar )
						{
						case ' ':
						case '"':
							break;
						default:
							arguments.emplace_back( argumentStart, curChar );

							break;
						}

						setCursor( argumentStart, curChar );

						// We've entered a comment, nothing left to do, so we bail
						return arguments;
					}
				}
				break;

			default:
				switch ( lastChar )
				{
				case ' ':
				case '"':
					argumentStart = curChar;
					break;
				}

				if ( nextChar == end )
				{
					arguments.emplace_back( argumentStart, end );
					setCursor( argumentStart, end );
				}

				break;
			}
		}

		lastChar = *curChar;
	}

	return arguments;
}

static std::tuple<int, bool> GetNextStatementLength( std::string_view line )
{
	bool skip = false;
	bool quoted = false;
	bool commented = false;
	int totalLength = line.length();
	int length = 0;

	auto curChar = line.begin();
	auto nextChar = curChar + 1;

	for ( ; curChar != line.end(); curChar++, length++ )
	{
		nextChar = curChar + 1;

		if ( !commented )
		{
			if ( *curChar == '"' )
			{
				quoted = !quoted;
				continue;
			}

			if ( !quoted )
			{
				if ( nextChar != line.end() )
				{
					if ( *curChar == '/' && *nextChar == '/' )
					{
						commented = true;
						continue;
					}
				}

				if ( *curChar == ';' )
				{
					skip = true;
					break;
				}
			}

			if ( *curChar == '\n' )
			{
				skip = true;
				break;
			}
		}
	}

	return { ( curChar != line.end() ? length : totalLength ), skip };
}

std::vector<std::string_view> CVarSystem::GetStatements( const std::string& input, size_t cursor, size_t& cursorIndex )
{
	auto begin = input.begin();
	auto end = input.end();
	std::vector<std::string_view> statements;

	std::string_view remaining( input );
	bool cursorReached = false;

	cursorIndex = -1;

	auto setCursor = [&]( size_t statementLength ) {
		if ( statementLength < cursor )
		{
			cursor -= statementLength;
		}
		else if ( !cursorReached )
		{
			cursorIndex = statements.size() - 1;
			cursorReached = true;
		}
	};

	while ( !remaining.empty() )
	{
		auto [length, skip] = GetNextStatementLength( remaining );
		int skip_length = skip ? length + 1 : length;

		std::string_view statement = remaining.substr( 0, length );

		const char* whitespace = " \t\r";

		size_t prefix = std::min( statement.find_first_not_of( whitespace ), statement.size() );
		statement.remove_prefix( prefix );

		size_t suffix = std::min( statement.size() - statement.find_last_not_of( whitespace ) - 1, statement.size() );
		statement.remove_suffix( suffix );

		if ( !statement.empty() )
			statements.push_back( statement );
		setCursor( skip_length );

		remaining = remaining.substr( skip_length, remaining.length() );
	}

	return statements;
}

std::vector<std::string> CVarSystem::GetStatementArguments( std::string_view statement )
{
	size_t cursorIndex;
	return GetStatementArguments( statement, 0, cursorIndex );
}

std::vector<std::string_view> CVarSystem::GetStatements( const std::string& input )
{
	size_t cursorIndex;
	return GetStatements( input, 0, cursorIndex );
}

#pragma endregion

void CVarSystem::Run( const char* input )
{
	std::string inputString = std::string( input );

	std::vector<std::string_view> statements = GetStatements( inputString );

	for ( std::string_view& statement : statements )
	{
		std::vector<std::string> arguments = GetStatementArguments( statement );

		if ( arguments.size() < 1 )
			continue;

		std::string& arg0 = arguments.at( 0 );

		if ( !Exists( arg0 ) )
		{
			spdlog::info( "{} is not a valid command or variable", arg0 );
			continue;
		}

		CVarEntry& entry = GetEntry( arg0 );

		if ( entry.IsCommand() )
		{
			auto passedArguments = std::vector<std::string>( arguments.begin() + 1, arguments.end() );
			entry.InvokeCommand( passedArguments );
		}
		else
		{
			if ( arguments.size() == 1 )
			{
				spdlog::info( "{} is '{}'", arg0, entry.ToString() );
			}
			else
			{
				// Guaranteed to be at least 2
				entry.FromString( arguments.at( 1 ) );
			}
		}
	}
}

void CVarSystem::RunFile( std::string fileName )
{
	std::ifstream file( fileName );

	if ( !file.is_open() )
	{
		spdlog::error( "Couldn't open '{}'", fileName );
		return;
	}

	std::string line;

	while ( std::getline( file, line ) )
	{
		Run( line.c_str() );
	}

	file.close();
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
	// This *has* to have the command flag
	flags = ( CVarFlags )( flags | CVarFlags::Command );

	CVarEntry entry = {};
	entry.m_name = name;
	entry.m_description = ( description != "" ) ? description : "(no description)";
	entry.m_flags = flags;
	entry.m_callback = callback;

	assert( entry.IsManaged() || callback != nullptr );

	size_t hash = GetHash( name );
	m_cvarEntries[hash] = entry;
}

template <typename T>
inline void CVarSystem::RegisterVariable(
    std::string name, T value, CVarFlags flags, std::string description, CVarCallback<T> callback )
{
	// This *must not* have the command flag
	flags = ( CVarFlags )( flags & ~( CVarFlags::Command ) );

	CVarEntry entry = {};
	entry.m_name = name;
	entry.m_description = ( description != "" ) ? description : "(no description)";
	entry.m_flags = flags;
	entry.m_value = value;
	entry.m_callback = callback;

	size_t hash = GetHash( name );
	m_cvarEntries[hash] = entry;
}

void CVarSystem::RegisterString(
    std::string name, std::string value, CVarFlags flags, std::string description, CVarCallback<std::string> callback )
{
	RegisterVariable<std::string>( name, value, flags, description, callback );
}

void CVarSystem::RegisterFloat(
    std::string name, float value, CVarFlags flags, std::string description, CVarCallback<float> callback )
{
	RegisterVariable<float>( name, value, flags, description, callback );
}

void CVarSystem::RegisterBool(
    std::string name, bool value, CVarFlags flags, std::string description, CVarCallback<bool> callback )
{
	RegisterVariable<bool>( name, value, flags, description, callback );
}

void CVarSystem::RegisterInt(
    std::string name, int value, CVarFlags flags, std::string description, CVarCallback<int> callback )
{
	RegisterVariable<int>( name, value, flags, description, callback );
}

void CVarSystem::Remove( std::string name )
{
	size_t hash = GetHash( name );
	m_cvarEntries.erase( hash );
}

void CVarEntry::InvokeCommand( std::vector<std::string> arguments )
{
	assert( IsCommand() );

	if ( IsManaged() )
	{
		std::vector<const char*> managedArguments;

		for ( auto& argument : arguments )
			managedArguments.push_back( argument.c_str() );

		CVarManagedCmdDispatchInfo info{ m_name.c_str(), managedArguments.data(), managedArguments.size() };

		FindInstance().m_hostManager->DispatchCommand( info );
	}
	else
	{
		auto callback = std::any_cast<CCmdCallback>( m_callback );

		if ( callback )
		{
			callback( arguments );
		}
	}
}

void CVarSystem::InvokeCommand( std::string name, std::vector<std::string> arguments )
{
	if ( !Exists( name ) )
	{
		spdlog::error( "Tried to invoke command '{}', but it doesn't exist!", name );
		return;
	}

	CVarEntry& entry = GetEntry( name );

	if ( !entry.IsCommand() )
	{
		spdlog::error( "Tried to invoke command '{}', but it's a variable!", name );
		return;
	}

	entry.InvokeCommand( arguments );
}

CVarFlags CVarSystem::GetFlags( std::string name )
{
	if ( !Exists( name ) )
	{
		return CVarFlags::None;
	}

	return ( CVarFlags )GetEntry( name ).m_flags;
}

// Putting this stuff in the header caused bad juju

template <typename T>
inline T CVarEntry::GetValue()
{
	if ( IsCommand() || m_value.type() != typeid( T ) )
	{
		return {};
	}

	return std::any_cast<T>( m_value );
}

template <typename T>
inline void CVarEntry::SetValue( T value )
{
	if ( IsCommand() || m_value.type() != typeid( T ) )
	{
		return;
	}

	T oldValue = std::any_cast<T>( m_value );
	m_value = value;

	if ( IsManaged() )
	{
		// This is kinda dirty

		if constexpr ( std::is_same<T, std::string>::value )
		{
			CVarManagedVarDispatchInfo<const char*> stringInfo{ m_name.c_str(), oldValue.c_str(), value.c_str() };

			FindInstance().m_hostManager->DispatchStringCVarCallback( stringInfo );
		}
		else if constexpr ( std::is_same<T, float>::value )
		{
			CVarManagedVarDispatchInfo<T> primitiveInfo{ m_name.c_str(), oldValue, value };

			FindInstance().m_hostManager->DispatchFloatCVarCallback( primitiveInfo );
		}
		else if constexpr ( std::is_same<T, bool>::value )
		{
			CVarManagedVarDispatchInfo<T> primitiveInfo{ m_name.c_str(), oldValue, value };

			FindInstance().m_hostManager->DispatchBoolCVarCallback( primitiveInfo );
		}
		else if constexpr ( std::is_same<T, int>::value )
		{
			CVarManagedVarDispatchInfo<T> primitiveInfo{ m_name.c_str(), oldValue, value };

			FindInstance().m_hostManager->DispatchIntCVarCallback( primitiveInfo );
		}
	}
	else
	{
		auto callback = std::any_cast<CVarCallback<T>>( m_callback );

		if ( callback )
		{
			callback( oldValue, value );
		}
	}

	spdlog::info( "{} was set to '{}'.", m_name, value );
}

std::string CVarEntry::GetString()
{
	return GetValue<std::string>();
}

std::string CVarSystem::GetString( std::string name )
{
	if ( !Exists( name ) )
	{
		return "";
	}

	return GetEntry( name ).GetString();
}

float CVarEntry::GetFloat()
{
	return GetValue<float>();
}

float CVarSystem::GetFloat( std::string name )
{
	if ( !Exists( name ) )
	{
		return 0.0f;
	}

	return GetEntry( name ).GetFloat();
}

bool CVarEntry::GetBool()
{
	return GetValue<bool>();
}

bool CVarSystem::GetBool( std::string name )
{
	if ( !Exists( name ) )
	{
		return false;
	}

	return GetEntry( name ).GetBool();
}

int CVarEntry::GetInt()
{
	return GetValue<int>();
}

int CVarSystem::GetInt( std::string name )
{
	if ( !Exists( name ) )
	{
		return false;
	}

	return GetEntry( name ).GetInt();
}

void CVarEntry::SetString( std::string value )
{
	SetValue<std::string>( value );
}

void CVarSystem::SetString( std::string name, std::string value )
{
	if ( !Exists( name ) )
	{
		return;
	}

	GetEntry( name ).SetString( value );
}

void CVarEntry::SetFloat( float value )
{
	SetValue<float>( value );
}

void CVarSystem::SetFloat( std::string name, float value )
{
	if ( !Exists( name ) )
	{
		return;
	}

	GetEntry( name ).SetFloat( value );
}

void CVarEntry::SetBool( bool value )
{
	SetValue<bool>( value );
}

void CVarSystem::SetBool( std::string name, bool value )
{
	if ( !Exists( name ) )
	{
		return;
	}

	GetEntry( name ).SetBool( value );
}

void CVarEntry::SetInt( int value )
{
	SetValue<int>( value );
}

void CVarSystem::SetInt( std::string name, int value )
{
	if ( !Exists( name ) )
	{
		return;
	}

	GetEntry( name ).SetInt( value );
}

std::string CVarEntry::ToString()
{
	const std::type_info& type = m_value.type();
	std::string valueStr;

	if ( type == typeid( std::string ) )
		valueStr = std::any_cast<std::string>( m_value );
	else if ( type == typeid( float ) )
		valueStr = std::to_string( std::any_cast<float>( m_value ) );
	else if ( type == typeid( bool ) )
		valueStr = std::any_cast<bool>( m_value ) ? "true" : "false";
	else if ( type == typeid( int ) )
		valueStr = std::to_string( std::any_cast<int>( m_value ) );

	return valueStr;
}

std::string CVarSystem::ToString( std::string name )
{
	if ( !Exists( name ) )
	{
		return "";
	}

	return GetEntry( name ).ToString();
}

void CVarEntry::FromString( std::string valueStr )
{
	std::stringstream valueStream( valueStr );

	auto& type = m_value.type();

	if ( type == typeid( float ) )
	{
		float value;
		valueStream >> value;

		SetValue<float>( value );
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

		SetValue<bool>( value );
	}
	else if ( type == typeid( std::string ) )
	{
		SetValue<std::string>( valueStr );
	}
	else if ( type == typeid( int ) )
	{
		float value;
		valueStream >> value;

		SetValue<int>( value );
	}
}

void CVarSystem::FromString( std::string name, std::string valueStr )
{
	if ( !Exists( name ) )
	{
		return;
	}

	GetEntry( name ).FromString( valueStr );
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

void CVarManager::Run( const char* input )
{
	CVarSystem::Instance().Run( input );
}

// ----------------------------------------
// Built-in CVars
// ----------------------------------------

static std::string GetFlagsString( CVarFlags flags )
{
	std::vector<const char*> flagNames;

	if ( flags & CVarFlags::Command )
		flagNames.push_back( "command" );
	else
		flagNames.push_back( "variable" );

	if ( flags & CVarFlags::Managed )
		flagNames.push_back( "managed" );

	if ( flags & CVarFlags::Game )
		flagNames.push_back( "game" );

	if ( flags & CVarFlags::Archive )
		flagNames.push_back( "archive" );

	if ( flags & CVarFlags::Cheat )
		flagNames.push_back( "cheat" );

	if ( flags & CVarFlags::Temp )
		flagNames.push_back( "temp" );

	if ( flags & CVarFlags::Replicated )
		flagNames.push_back( "replicated" );

	std::stringstream ss;

	size_t len = flagNames.size();
	for ( int i = 0; i < len; i++ )
	{
		ss << flagNames[i];
		if ( i != len - 1 )
			ss << ", ";
	}

	return ss.str();
}

static CCmd ccmd_list( "list", CVarFlags::None, "List all commands and variables", []( std::vector<std::string> arguments ) {
	auto instance = CVarSystem::Instance();

// This fails on libclang so we'll ignore it for now...
#ifndef __clang__
	// List all available cvars
	instance.ForEach( [&]( CVarEntry& entry ) {
		std::string flagNames = GetFlagsString( ( CVarFlags )entry.m_flags );

		if ( entry.IsCommand() )
			spdlog::info( "- '{}' - {}", entry.m_name, flagNames );
		else
			spdlog::info( "- '{}': '{}' - {}", entry.m_name, entry.ToString(), flagNames );
		spdlog::info( "\t{}", entry.m_description );
	} );
#endif
} );

// ----------------------------------------
// Test CVars
// ----------------------------------------

static FloatCVar cvartest_float( "cvartest.float", 0.0f, CVarFlags::None, "Yeah",
    []( float oldValue, float newValue ) { spdlog::trace( "cvartest.float changed! old {}, new {}", oldValue, newValue ); } );

static CCmd cvartest_command( "cvartest.command", CVarFlags::None, "A test command", []( std::vector<std::string> arguments ) {
	spdlog::trace( "cvartest.command has been invoked! Hooray" );

	for ( int i = 0; i < arguments.size(); i++ )
	{
		spdlog::trace( "\t{} - '{}'", i, arguments.at( i ) );
	}
} );