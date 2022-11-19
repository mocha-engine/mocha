#include "logger.h"

#include "spdlog/spdlog.h"

#include <iostream>

Logger::Logger() {}

void Logger::Info( std::string str )
{
	spdlog::get( "managed" )->info( str );
}

void Logger::Warning( std::string str )
{
	spdlog::get( "managed" )->warn( str );
}

void Logger::Error( std::string str )
{
	spdlog::get( "managed" )->error( str );
}

void Logger::Trace( std::string str )
{
	spdlog::get( "managed" )->trace( str );
}