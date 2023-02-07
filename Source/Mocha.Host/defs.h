#pragma once
#include <cvarmanager.h>

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
	__debugbreak();
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
// Engine features
//
namespace EngineProperties
{
	extern StringCVar LoadedProject;
	extern BoolCVar Raytracing;
	extern BoolCVar Renderdoc;
};

//
// Engine properties
//
#define ENGINE_NAME						"Mocha"
#define WINDOW_TITLE					std::string( g_projectManager->GetProject().name + " [" + g_projectManager->GetProject().version + "]" ).c_str()

//
// Types
//
typedef uint32_t Handle;
#define HANDLE_INVALID					UINT32_MAX

// clang-format on