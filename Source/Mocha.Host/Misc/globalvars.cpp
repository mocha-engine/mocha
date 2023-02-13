#include "globalvars.h"

#include <Misc/cvarmanager.h>
#include <Misc/mathtypes.h>
#include <Root/clientroot.h>
#include <Root/root.h>

//
// Engine features
//
namespace EngineProperties
{
	StringCVar LoadedProject(
	    "project.current", "Samples\\mocha-minimal\\project.json", CVarFlags::Archive, "Which project should we load?" );
	BoolCVar Raytracing( "render.raytracing", true, CVarFlags::Archive, "Enable raytracing" );
	BoolCVar Renderdoc( "render.renderdoc", false, CVarFlags::Archive, "Enable renderdoc" );

	StringCVar ServerName( "server.name", "Mocha Dedicated Server", CVarFlags::None, "Server name" );
	StringCVar ServerPassword( "server.password", "", CVarFlags::None, "Server password" );
	IntCVar ServerPort( "server.port", 7777, CVarFlags::None, "Server port" );
	IntCVar ServerMaxPlayers( "server.maxplayers", 16, CVarFlags::None, "Server max players" );
	FloatCVar timescale( "game.timescale", 1.0f, CVarFlags::Archive, "The speed at which the game world runs." );
} // namespace EngineProperties