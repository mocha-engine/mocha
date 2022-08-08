#pragma once
#include <spdlog/spdlog.h>

#define ASSERT( x )                                                               \
	if ( FAILED( x ) )                                                            \
	{                                                                             \
		spdlog::error( "Assertion failed at {}:{}\n{}", __FILE__, __LINE__, #x ); \
		__debugbreak();                                                           \
	}