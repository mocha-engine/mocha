#include "CLogger.h"

#include "spdlog/spdlog.h"
#include <iostream>

void CLogger::Info(const char* str)
{
	spdlog::info(str);
}

void CLogger::Warning(const char* str)
{
	spdlog::warn(str);
}

void CLogger::Error(const char* str)
{
	spdlog::error(str);
}

void CLogger::Trace(const char* str)
{
	spdlog::trace(str);
}
