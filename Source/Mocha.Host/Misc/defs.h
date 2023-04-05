#pragma once
#include <stdint.h>
#include <string>

// clang-format off

//
// Helper macros
//
#define ADD_QUOTES_HELPER( s ) #s
#define ADD_QUOTES( s ) ADD_QUOTES_HELPER( s )

#if defined( __clang__ )
// We only need this to be accessible from within InteropGen, which runs off libclang.
#define GENERATE_BINDINGS __attribute__((annotate("generate_bindings")))
#else
#define GENERATE_BINDINGS
#endif

#if _WIN32
#include <windows.h>
#include <source_location>

#if defined( __clang__ )
// Clang does not currently support std::source_location, so we'll define an empty function for InteropGen
inline void ErrorMessage( std::string str ) {}
#else
// Display message box
inline void ErrorMessage( std::string str, const std::source_location& location = std::source_location::current() )
{
	MessageBoxA( nullptr, str.c_str(), "Engine Error", MB_OK | MB_ICONERROR );
	printf( "Engine Error %s occurred at line %d in file %s", str.c_str(), location.line(), location.file_name() );
}
#endif

#else
#pragma error "Unsupported platform"
#endif

#define DEFINE_FLAG_OPERATORS(ENUMTYPE) \
extern "C++" { \
inline ENUMTYPE operator | (ENUMTYPE a, ENUMTYPE b) { return ENUMTYPE(((int)a) | ((int)b)); } \
inline ENUMTYPE &operator |= (ENUMTYPE &a, ENUMTYPE b) { return (ENUMTYPE &)(((int &)a) |= ((int)b)); } \
inline ENUMTYPE operator & (ENUMTYPE a, ENUMTYPE b) { return ENUMTYPE(((int)a) & ((int)b)); } \
inline ENUMTYPE &operator &= (ENUMTYPE &a, ENUMTYPE b) { return (ENUMTYPE &)(((int &)a) &= ((int)b)); } \
inline ENUMTYPE operator ~ (ENUMTYPE a) { return ENUMTYPE(~((int)a)); } \
inline ENUMTYPE operator ^ (ENUMTYPE a, ENUMTYPE b) { return ENUMTYPE(((int)a) ^ ((int)b)); } \
inline ENUMTYPE &operator ^= (ENUMTYPE &a, ENUMTYPE b) { return (ENUMTYPE &)(((int &)a) ^= ((int)b)); } \
}

//
// Engine properties
//
#define ENGINE_NAME						"Mocha"

//
// Types
//
typedef uint32_t Handle;
#define HANDLE_INVALID					UINT32_MAX

// TODO: Remove
enum RenderDebugViews
{
	NONE = 0,
	DIFFUSE = 1,
	NORMAL = 2,
	AMBIENTOCCLUSION = 3,
	METALNESS = 4,
	ROUGHNESS = 5,

	OTHER = 63
};

enum Realm
{
	REALM_SERVER,
	REALM_CLIENT
};

inline const char* RealmToString( const Realm& realm )
{
	switch ( realm )
	{
	case REALM_SERVER:
		return "Server";
	case REALM_CLIENT:
		return "Client";
	}

	__debugbreak();
	return "Unknown";
}

// clang-format on