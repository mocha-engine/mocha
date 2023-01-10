#pragma once

#include <cvarmanager.h>
#include <fstream>
#include <nlohmann/json.hpp>
#include <string>

namespace EngineProperties
{
	extern StringCVar GameConfig;
} // namespace EngineProperties

struct ManagedSettings
{
	std::string path;
	std::string signature;
};

struct Settings
{
	std::string name = "Unnamed Game";
	std::string icon = "";
	std::string milestone = "Prototype";

	int tickRate = 60;

	ManagedSettings managed = {};

	void Deserialize( const nlohmann::json& j )
	{
		name = j["name"].get<std::string>();
		icon = j["icon"].get<std::string>();
		milestone = j["milestone"].get<std::string>();
		tickRate = j["tickRate"].get<int>();

		managed = {};
		managed.path = j["managed"]["path"].get<std::string>();
		managed.signature = j["managed"]["signature"].get<std::string>();
	}

	Settings( std::string path )
	{
		nlohmann::json j;

		std::ifstream cvarFile( path );
		cvarFile >> j;

		Deserialize( j );
	}
};

class GameSettings
{
public:
	static Settings* Get()
	{
		static Settings settings( EngineProperties::GameConfig );

		return &settings;
	}
}; // namespace GameSettings