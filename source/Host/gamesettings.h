#pragma once

#include <fstream>
#include <nlohmann/json.hpp>
#include <string>

struct GameFeatures
{
	bool raytracing;
};

struct Settings
{
	std::string name;
	std::string icon;
	std::string milestone;

	GameFeatures features;

	void Deserialize( const nlohmann::json& j )
	{
		name = j["name"].get<std::string>();
		icon = j["icon"].get<std::string>();
		milestone = j["milestone"].get<std::string>();

		features.raytracing = j["features"]["raytracing"].get<bool>();
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
		static Settings settings( "spacegame.json" );

		return &settings;
	}
}; // namespace GameSettings