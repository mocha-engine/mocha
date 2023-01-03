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

// Display message box
#define ERRORMESSAGE( x ) MessageBoxA( nullptr, x.c_str(), ENGINE_NAME " Error", MB_OK | MB_ICONERROR )
#else
#pragma error "Unsupported platform"
#endif

//
// Engine features
//
namespace EngineFeatures
{
	#define ENGINE_FEATURE( name, value )\
		static bool name = value;

	ENGINE_FEATURE( Raytracing, GameSettings::Get()->features.raytracing )
};

//
// Game properties
//
#define GAME_MILESTONE					"Prototype"
#define GAME_VERSION					ADD_QUOTES( GIT_CUR_COMMIT ) " on " ADD_QUOTES( GIT_BRANCH )
#define MANAGED_PATH					L".\\build\\Engine"
#define MANAGED_CLASS					L"Mocha.Main, Engine"

//
// Engine properties
//
#define ENGINE_NAME						"Mocha"
#define WINDOW_TITLE					std::string( GameSettings::Get()->name + " [" + GameSettings::Get()->milestone + "] - " GAME_VERSION ).c_str()

//
// Types
//
typedef uint32_t Handle;

// clang-format on