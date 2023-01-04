#pragma once
#include <cvarmanager.h>
#include <gamesettings.h>
#include <gitdefs.h>

// clang-format off

//
// Helper macros
//
#define ADD_QUOTES_HELPER( s ) #s
#define ADD_QUOTES( s ) ADD_QUOTES_HELPER( s )

#if _WIN32
#include <windows.h>
#include <source_location>

// Display message box
inline void ErrorMessage( std::string str, const std::source_location& location = std::source_location::current() )
{
	MessageBoxA( nullptr, str.c_str(), "Engine Error", MB_OK | MB_ICONERROR );
	printf( "Engine Error %s occurred at line %d in file %s", str.c_str(), location.line(), location.file_name() );
	__debugbreak();
}
#else
#pragma error "Unsupported platform"
#endif

//
// Engine features
//
namespace EngineProperties
{
	extern BoolCVar Raytracing;
	extern BoolCVar Renderdoc;
};

//
// Engine properties
//
#define ENGINE_NAME						"Mocha"
#define GAME_VERSION					ADD_QUOTES( GIT_CUR_COMMIT ) " on " ADD_QUOTES( GIT_BRANCH )
#define WINDOW_TITLE					std::string( GameSettings::Get()->name + " [" + GameSettings::Get()->milestone + "] - " GAME_VERSION ).c_str()

//
// Types
//
typedef uint32_t Handle;
#define HANDLE_INVALID					UINT32_MAX

// clang-format on