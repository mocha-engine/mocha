#include "logger.h"

#include "spdlog/spdlog.h"

#include <iostream>

CLogger::CLogger() {}

void CLogger::Info( std::string str )
{
	spdlog::info( str );
}

void CLogger::Warning( std::string str )
{
	spdlog::warn( str );
}

void CLogger::Error( std::string str )
{
	spdlog::error( str );
}

void CLogger::Trace( std::string str )
{
	spdlog::trace( str );
}