#include "logger.h"

#include "spdlog/spdlog.h"

#include <iostream>

CLogger::CLogger() {}

void CLogger::Info( std::string str )
{
	spdlog::get( "managed" )->info( str );
}

void CLogger::Warning( std::string str )
{
	spdlog::get( "managed" )->warn( str );
}

void CLogger::Error( std::string str )
{
	spdlog::get( "managed" )->error( str );
}

void CLogger::Trace( std::string str )
{
	spdlog::get( "managed" )->trace( str );
}