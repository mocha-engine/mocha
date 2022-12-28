#pragma once

#include <fstream>
#include <nlohmann/json.hpp>
#include <string>

struct GameSettings
{
	std::string gameName;
	std::string icon;
	std::string version;

	nlohmann::json Serialize() { return nlohmann::json{ { "gameName", gameName }, { "icon", icon }, { "version", version } }; }

	void Deserialize( const nlohmann::json& j )
	{
		gameName = j["gameName"].get<std::string>();
		icon = j["icon"].get<std::string>();
		version = j["version"].get<std::string>();
	}

	GameSettings( std::string path )
	{
		nlohmann::json j;

		std::ifstream cvarFile( path );
		cvarFile >> j;

		Deserialize( j );
	}
};