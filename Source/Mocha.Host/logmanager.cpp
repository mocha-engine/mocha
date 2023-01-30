#include "logmanager.h"

#include "spdlog/spdlog.h"

#include <iostream>

void LogManager::Startup()
{
	// Setup spdlog
	auto mochaSink = std::make_shared<MochaSinkMT>();
	auto managed = std::make_shared<spdlog::logger>( "managed", mochaSink );
	auto main = std::make_shared<spdlog::logger>( "main", mochaSink );
	auto renderer = std::make_shared<spdlog::logger>( "renderer", mochaSink );

	spdlog::register_logger( managed );
	spdlog::register_logger( main );
	spdlog::register_logger( renderer );

	spdlog::set_default_logger( main );
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