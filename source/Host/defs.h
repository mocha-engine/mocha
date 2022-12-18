#pragma once
#include <gitdefs.h>

#define ADD_QUOTES_HELPER( s ) #s
#define ADD_QUOTES( s ) ADD_QUOTES_HELPER( s )

// clang-format off
#define MANAGED_PATH					L".\\build\\Engine"
#define MANAGED_CLASS					L"Mocha.Main, Engine"

#define ENGINE_NAME						"Mocha"
#define VERSION_NAME					ADD_QUOTES( GIT_CUR_COMMIT ) " on " ADD_QUOTES( GIT_BRANCH )
#define GAME_NAME						"SpaceGame [Prototype] - " VERSION_NAME
// clang-format on