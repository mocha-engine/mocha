#pragma once
#include <spdlog/spdlog.h>

#define FAILED( x ) ( ( ( int )( x ) ) < 0 )

#if _DEBUG
#define ASSERT( x )                                                                             \
	if ( FAILED( x ) )                                                                          \
	{                                                                                           \
		spdlog::error( "Assertion failed at {}:{}\n{}\nError: {}", __FILE__, __LINE__, #x, x ); \
		__debugbreak();                                                                         \
		throw std::exception();                                                                 \
	}
#else
#define ASSERT( x )                            \
	if ( FAILED( x ) )                         \
	{                                          \
		spdlog::error( "Fatal error: {}", x ); \
		throw std::exception();                \
	}
#endif