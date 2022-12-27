#pragma once
#include <gitdefs.h>

#define ADD_QUOTES_HELPER( s ) #s
#define ADD_QUOTES( s ) ADD_QUOTES_HELPER( s )

// clang-format off

//
// Engine features
//
#define RAYTRACING						1

//
// Game properties
//
#define GAME_NAME						"SpaceGame"
#define GAME_MILESTONE					"Prototype"
#define GAME_VERSION					ADD_QUOTES( GIT_CUR_COMMIT ) " on " ADD_QUOTES( GIT_BRANCH )
#define MANAGED_PATH					L".\\build\\Engine"
#define MANAGED_CLASS					L"Mocha.Main, Engine"

//
// Engine properties
//
#define ENGINE_NAME						"Mocha"
#define WINDOW_TITLE					GAME_NAME " [" GAME_MILESTONE "] - " GAME_VERSION

// clang-format on

#if _WIN32
#include <windows.h>

// Display message box
#define ERRORMESSAGE( x ) MessageBoxA( nullptr, x.c_str(), ENGINE_NAME " Error", MB_OK | MB_ICONERROR )
#else
#pragma error "Unsupported platform"
#endif