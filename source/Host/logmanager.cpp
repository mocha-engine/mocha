#include "logmanager.h"

#include "spdlog/spdlog.h"

#include <iostream>

LogManager::LogManager() {}

void LogManager::Startup()
{
	// Setup spdlog
	auto managed = spdlog::stdout_color_mt( "managed" );
	auto main = spdlog::stderr_color_mt( "main" );
	auto renderer = spdlog::stderr_color_mt( "renderer" );
	spdlog::set_default_logger( main );
	spdlog::set_level( spdlog::level::trace );

	// Set pattern "time logger,8 type,8 message"
	spdlog::set_pattern( "%H:%M:%S %-8n %^%-8l%$ %v" );
}

void LogManager::Shutdown() {}

void LogManager::Info( std::string str )
{
	spdlog::get( "managed" )->info( str );
}

void LogManager::Warning( std::string str )
{
	spdlog::get( "managed" )->warn( str );
}

void LogManager::Error( std::string str )
{
	spdlog::get( "managed" )->error( str );
}

void LogManager::Trace( std::string str )
{
	spdlog::get( "managed" )->trace( str );
}