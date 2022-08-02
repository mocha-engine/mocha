#include "CLogger.h"

#include "spdlog/spdlog.h"
#include <iostream>

CLogger::CLogger()
{
}

CLogger::~CLogger()
{
}

void CLogger::Log(const char* str)
{
	spdlog::info(str);
}

int* CLogger::InteropTest(const char* a, const char* b, int* c)
{
	return nullptr;
}

CLogger* CLogger::GetActiveLogger()
{
	return nullptr;
}
