#pragma once
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>

//@InteropGen generate class
class LogManager : ISubSystem
{
public:
	LogManager();

	void StartUp();
	void ShutDown();

	void Info( std::string str );
	void Warning( std::string str );
	void Error( std::string str );
	void Trace( std::string str );
};