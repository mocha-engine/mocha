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
	if ( !spdlog::get( "managed" ) )
	{
		auto managed = std::make_shared<spdlog::logger>( "managed", mochaSink );
		spdlog::register_logger( managed );
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
	spdlog::get( "managed" )->info( str );
}

void LogManager::ManagedWarning( std::string str )
{
	spdlog::get( "managed" )->warn( str );
}

void LogManager::ManagedError( std::string str )
{
	spdlog::get( "managed" )->error( str );
}

void LogManager::ManagedTrace( std::string str )
{
	spdlog::get( "managed" )->trace( str );
}