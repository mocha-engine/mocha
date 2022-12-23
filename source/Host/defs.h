#pragma once
#include <gitdefs.h>

#define ADD_QUOTES_HELPER( s ) #s
#define ADD_QUOTES( s ) ADD_QUOTES_HELPER( s )

// clang-format off
#define MANAGED_PATH					L".\\build\\Engine"
#define MANAGED_CLASS					L"Mocha.Main, Engine"

#define ENGINE_NAME						"Mocha"
#define GAME_NAME						"SpaceGame"
#define GAME_MILESTONE					"Prototype"
#define GAME_VERSION					ADD_QUOTES( GIT_CUR_COMMIT ) " on " ADD_QUOTES( GIT_BRANCH )
#define WINDOW_TITLE					GAME_NAME " [" GAME_MILESTONE "] - " GAME_VERSION
// clang-format on