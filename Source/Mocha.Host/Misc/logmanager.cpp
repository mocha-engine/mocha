#include "logmanager.h"

#include "spdlog/spdlog.h"

#include <iostream>

void LogManager::Startup()
{
	// Only do this once per-app - loggers are shared between
	// roots, so we don't want to create multiple loggers
	if ( IsInitialized.load() )
		return;

	IsInitialized.store( true );

	// Setup spdlog
	auto mochaSink = std::make_shared<MochaSinkMT>();

	// Register loggers if they don't exist
	if ( !spdlog::get( "managed-cl" ) )
	{
		auto logger = std::make_shared<spdlog::logger>( "managed-cl", mochaSink );
		spdlog::register_logger( logger );
	}

	if ( !spdlog::get( "managed-sv" ) )
	{
		auto logger = std::make_shared<spdlog::logger>( "managed-sv", mochaSink );
		spdlog::register_logger( logger );
	}

	if ( !spdlog::get( "main" ) )
	{
		auto main = std::make_shared<spdlog::logger>( "main", mochaSink );
		spdlog::register_logger( main );
		spdlog::set_default_logger( main );
	}

	spdlog::set_level( spdlog::level::trace );

	// Set pattern "time logger,8 type,8 message"
	spdlog::set_pattern( "%H:%M:%S %-8n %^%-8l%$ %v" );
}

void LogManager::ManagedInfo( std::string str )
{
	// TODO: executingRealm should probably be specific to the host, and we should probably specify the name of the logger from
	// within managed code
	if ( Globals::m_executingRealm == REALM_CLIENT )
		spdlog::get( "managed-cl" )->info( str );

	if ( Globals::m_executingRealm == REALM_SERVER )
		spdlog::get( "managed-sv" )->info( str );
}

void LogManager::ManagedWarning( std::string str )
{
	if ( Globals::m_executingRealm == REALM_CLIENT )
		spdlog::get( "managed-cl" )->warn( str );

	if ( Globals::m_executingRealm == REALM_SERVER )
		spdlog::get( "managed-sv" )->warn( str );
}

void LogManager::ManagedError( std::string str )
{
	if ( Globals::m_executingRealm == REALM_CLIENT )
		spdlog::get( "managed-cl" )->error( str );

	if ( Globals::m_executingRealm == REALM_SERVER )
		spdlog::get( "managed-sv" )->error( str );
}

void LogManager::ManagedTrace( std::string str )
{
	if ( Globals::m_executingRealm == REALM_CLIENT )
		spdlog::get( "managed-cl" )->trace( str );

	if ( Globals::m_executingRealm == REALM_SERVER )
		spdlog::get( "managed-sv" )->trace( str );
}