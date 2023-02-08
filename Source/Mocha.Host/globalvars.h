#pragma once

//
// Engine features
//
class StringCVar;
class BoolCVar;
class IntCVar;
class FloatCVar;

// TODO: move
namespace EngineProperties
{
	extern StringCVar LoadedProject;
	extern BoolCVar Raytracing;
	extern BoolCVar Renderdoc;

	extern StringCVar ServerHostname;
	extern StringCVar ServerPassword;
	extern IntCVar ServerPort;
	extern IntCVar ServerMaxPlayers;
}; // namespace EngineProperties

// TODO: Server / client
extern FloatCVar maxFramerate;