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
			FromString( entry, value );
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

std::vector<std::string_view> CVarSystem::GetStatements( const std::string& line, size_t cursor, size_t& cursorIndex )
{
	auto begin = line.begin();
	auto end = line.end();
	std::vector<std::string_view> statements;

	std::string_view remaining( line );
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

std::vector<std::string_view> CVarSystem::GetStatements( const std::string& line )
{
	size_t cursor = 0, cursorIndex;
	return GetStatements( line, cursor, cursorIndex );
}

#pragma endregion

void CVarSystem::Run( const char* command )
{
	std::string inputString = std::string( command );

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

		if ( entry.m_flags & CVarFlags::Command )
		{
			auto passedArguments = std::vector<std::string>( arguments.begin() + 1, arguments.end() );
			InvokeCommand( entry, passedArguments );
		}
		else
		{
			if ( arguments.size() == 1 )
			{
				spdlog::info( "{} is '{}'", arg0, ToString( entry ) );
			}
			else
			{
				// Guaranteed to be at least 2
				FromString( entry, arguments.at( 1 ) );
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
// Built-in CVars
// ----------------------------------------

static CCmd ccmd_list( "list", CVarFlags::None, "List all commands and variables",
	[]( std::vector<std::string> arguments )
	{
		auto instance = CVarSystem::Instance();

// This fails on libclang so we'll ignore it for now...
#ifndef __clang__
		// List all available cvars
		instance.ForEach( [&]( CVarEntry& entry ) {
		    if ( entry.m_flags & CVarFlags::Command )
			    spdlog::info( "- '{}' (command)", entry.m_name );
		    else
				spdlog::info( "- '{}': '{}' (variable)", entry.m_name, instance.ToString( entry ) );
			spdlog::info( "\t{}", entry.m_description );
		} );
#endif
	}
);

// ----------------------------------------
// Test CVars
// ----------------------------------------

static FloatCVar cvartest_float( "cvartest.float", 0.0f, CVarFlags::None, "Yeah",
	[]( float oldValue, float newValue )
	{
		spdlog::trace( "cvartest.float changed! old {}, new {}", oldValue, newValue );
	}
);

static CCmd cvartest_command( "cvartest.command", CVarFlags::None, "A test command",
	[]( std::vector<std::string> arguments )
	{
		spdlog::trace( "cvartest.command has been invoked! Hooray" );
		
		for ( int i = 0; i < arguments.size(); i++ )
		{
		    spdlog::trace( "\t{} - '{}'", i, arguments.at( i ) );
		}
	}
);