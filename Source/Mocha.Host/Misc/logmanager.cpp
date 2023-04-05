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
	m_sink = std::make_shared<MochaSinkMT>();

	// Register loggers if they don't exist
	if ( !spdlog::get( "core" ) )
	{
		auto coreLogger = std::make_shared<spdlog::logger>( "core", m_sink );
		spdlog::register_logger( coreLogger );
		spdlog::set_default_logger( coreLogger );
	}

	spdlog::set_level( spdlog::level::trace );

	// Set pattern "time logger,8 type,8 message"
	spdlog::set_pattern( "%H:%M:%S %-8n %^%-8l%$ %v" );
}

void LogManager::ManagedInfo( std::string loggerName, std::string str )
{
	GetLogger( loggerName )->info( str );
}

void LogManager::ManagedWarning( std::string loggerName, std::string str )
{
	GetLogger( loggerName )->warn( str );
}

void LogManager::ManagedError( std::string loggerName, std::string str )
{
	GetLogger( loggerName )->error( str );
}

void LogManager::ManagedTrace( std::string loggerName, std::string str )
{
	GetLogger( loggerName )->trace( str );
}